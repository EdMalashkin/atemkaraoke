﻿using BMDSwitcherAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SwitcherLib
{
	public partial class Switcher
	{
		public static IBMDSwitcherMixEffectBlock m_mixEffectBlock1;
		public static IBMDSwitcherMixEffectBlock m_mixEffectBlock2;
		protected IBMDSwitcher switcher;
		protected string deviceAddress;
		protected bool connected;


		public Switcher(string deviceAddress)
		{
			this.deviceAddress = deviceAddress;
		}

		public IBMDSwitcher GetSwitcher()
		{
			return this.switcher;
		}

		public void Connect()
		{
			if (this.connected)
			{
				return;
			}

			IBMDSwitcherDiscovery switcherDiscovery = new CBMDSwitcherDiscovery();
			_BMDSwitcherConnectToFailure failReason = 0;

			try
			{
				switcherDiscovery.ConnectTo(this.deviceAddress, out this.switcher, out failReason);
				this.connected = true;

				// Get the first Mix Effect block (ME 1). 
				m_mixEffectBlock1 = null;

				IBMDSwitcherMixEffectBlockIterator meIterator = null;
				IntPtr meIteratorPtr;
				Guid meIteratorIID = typeof(IBMDSwitcherMixEffectBlockIterator).GUID;
				switcher.CreateIterator(ref meIteratorIID, out meIteratorPtr);
				if (meIteratorPtr != null)
				{
					meIterator = (IBMDSwitcherMixEffectBlockIterator)Marshal.GetObjectForIUnknown(meIteratorPtr);
				}

				if (meIterator == null)
					return;

				if (meIterator != null)
				{
					meIterator.Next(out m_mixEffectBlock1);
				}

				if (m_mixEffectBlock1 == null)
				{
					throw new SwitcherLibException("Unexpected: Could not get first mix effect block");

				}

				// Get the second Mix Effect block (ME 2). 
				m_mixEffectBlock2 = null;
				if (meIterator != null)
				{
					meIterator.Next(out m_mixEffectBlock2);
				}


			}
			catch (COMException ex)
			{
				switch (failReason)
				{
					case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureIncompatibleFirmware:
						throw new SwitcherLibException("Incompatible firmware");

					case _BMDSwitcherConnectToFailure.bmdSwitcherConnectToFailureNoResponse:
						throw new SwitcherLibException(String.Format("No response from {0}", this.deviceAddress));

					default:
						throw new SwitcherLibException(String.Format("Unknown Error: {0}", ex.Message));
				}
			}
			catch (Exception ex)
			{
				throw new SwitcherLibException(String.Format("Unable to connect to switcher: {0}", ex.Message));
			}
		}
		public String GetProductName()
		{
			this.Connect();
			String productName;
			this.switcher.GetProductName(out productName);
			return productName;
		}
		public String GetVideoMode()
		{
			this.Connect();
			_BMDSwitcherVideoMode videoMode;
			this.switcher.GetVideoMode(out videoMode);
			return videoMode.ToString();
		}
		public int GetVideoHeight()
		{
			this.Connect();
			_BMDSwitcherVideoMode videoMode;
			this.switcher.GetVideoMode(out videoMode);
			_BMDSwitcherVideoMode switcherVideoMode = videoMode;
			switch (switcherVideoMode)
			{
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2398:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp24:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp25:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode4KHDp2997:
					return 2160;

				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p50:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode720p5994:
					return 720;

				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i50:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080i5994:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p50:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2398:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p24:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p25:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p2997:
				case _BMDSwitcherVideoMode.bmdSwitcherVideoMode1080p5994:
					return 1080;

				default:
					throw new SwitcherLibException(String.Format("Unsupported resolution: {0}", videoMode.ToString()));
			}
		}

		public int GetVideoWidth()
		{
			int videoHeight = this.GetVideoHeight();
			switch (videoHeight)
			{
				case 720:
					return 1280;

				case 1080:
					return 1920;

				case 2160:
					return 3840;

				default:
					throw new SwitcherLibException(String.Format("Unsupported video height: {0}", videoHeight.ToString()));
			}
		}

		public IList<MediaStill> GetStills()
		{
			IList<MediaStill> list = new List<MediaStill>();

			IBMDSwitcherMediaPool switcherMediaPool = (IBMDSwitcherMediaPool)this.switcher;

			IBMDSwitcherStills stills;
			switcherMediaPool.GetStills(out stills);

			uint count;
			stills.GetCount(out count);
			for (uint index = 0; index < count; index++)
			{
				MediaStill mediaStill = new MediaStill(stills, index);
				list.Add(mediaStill);
			}

			IntPtr mediaPlayerIteratorPtr;
			Guid mediaIteratorIID = typeof(IBMDSwitcherMediaPlayerIterator).GUID;
			this.switcher.CreateIterator(ref mediaIteratorIID, out mediaPlayerIteratorPtr);
			IBMDSwitcherMediaPlayerIterator mediaPlayerIterator = (IBMDSwitcherMediaPlayerIterator)Marshal.GetObjectForIUnknown(mediaPlayerIteratorPtr);

			IBMDSwitcherMediaPlayer mediaPlayer;
			mediaPlayerIterator.Next(out mediaPlayer);
			int num1 = 1;
			while (mediaPlayer != null)
			{
				_BMDSwitcherMediaPlayerSourceType type;
				uint index;
				mediaPlayer.GetSource(out type, out index);
				if (type == _BMDSwitcherMediaPlayerSourceType.bmdSwitcherMediaPlayerSourceTypeStill)
				{
					int num2 = (int)index + 1;
					foreach (MediaStill mediaStill in list)
					{
						if (mediaStill.Slot == num2)
						{
							mediaStill.MediaPlayer = num1;
							break;
						}
					}
				}
				num1++;
				mediaPlayerIterator.Next(out mediaPlayer);
			}
			return list;
		}

		//public class ProgramInput
		//{
		//	private long InputId;
		//	public int me { get; set; }
		//	public long inputId
		//	{
		//		get
		//		{
		//			switch (me)
		//			{
		//				case 2:
		//					m_mixEffectBlock2.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out InputId);
		//					return InputId;
		//				default:
		//					m_mixEffectBlock1.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, out InputId);
		//					return InputId;
		//			}
		//		}
		//		set
		//		{
		//			switch (me)
		//			{
		//				case 2:
		//					if (m_mixEffectBlock2 != null)
		//					{
		//						m_mixEffectBlock2.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, value);
		//					}
		//					break;
		//				default:
		//					if (m_mixEffectBlock1 != null)
		//					{
		//						m_mixEffectBlock1.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdProgramInput, value);
		//					}
		//					break;
		//			}
		//		}
		//	}
		//}
		//public class PreviewInput
		//{
		//	private long InputId;
		//	public int me
		//	{
		//		get; set;
		//	}
		//	public long inputId
		//	{
		//		get
		//		{
		//			switch (me)
		//			{
		//				case 2:
		//					m_mixEffectBlock2.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out InputId);
		//					return InputId;
		//				default:
		//					m_mixEffectBlock1.GetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, out InputId);
		//					return InputId;

		//			}

		//		}
		//		set
		//		{
		//			switch (me)
		//			{
		//				case 2:
		//					if (m_mixEffectBlock2 != null)
		//					{
		//						m_mixEffectBlock2.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, value);
		//					}
		//					break;
		//				default:
		//					if (m_mixEffectBlock1 != null)
		//					{
		//						m_mixEffectBlock1.SetInt(_BMDSwitcherMixEffectBlockPropertyId.bmdSwitcherMixEffectBlockPropertyIdPreviewInput, value);
		//					}
		//					break;
		//			}
		//		}
		//	}
		//}

		public void Cut(int me = 1)
		{
			switch (me)
			{
				case 1:
					if (m_mixEffectBlock1 != null)
					{
						m_mixEffectBlock1.PerformCut();
					}
					break;
				case 2:
					if (m_mixEffectBlock2 != null)
					{
						m_mixEffectBlock2.PerformCut();
					}
					break;
			}

		}
		public void AutoTransition(int me = 1, double dTransitionFrames = 0)
		{
			switch (me)
			{
				case 1:
					if (m_mixEffectBlock1 != null)
					{
						if (dTransitionFrames != 0)
						{
							//double dTransitionFrames = dFramesPerSecond * dTransitionSeconds;
							// ToDo - get current transition rate and then restore it after end of autotransition.
							BMDSwitcherAPI.IBMDSwitcherTransitionMixParameters m_params =
							(BMDSwitcherAPI.IBMDSwitcherTransitionMixParameters)m_mixEffectBlock1;

							m_params.SetRate((uint)dTransitionFrames);
						}
						m_mixEffectBlock1.PerformAutoTransition();
					}
					break;
				case 2:
					if (m_mixEffectBlock2 != null)
					{
						if (dTransitionFrames != 0)
						{
							//double dTransitionFrames = dFramesPerSecond * dTransitionSeconds;
							// ToDo - get current transition rate and then restore it after end of autotransition.
							BMDSwitcherAPI.IBMDSwitcherTransitionMixParameters m_params =
							(BMDSwitcherAPI.IBMDSwitcherTransitionMixParameters)m_mixEffectBlock2;

							m_params.SetRate((uint)dTransitionFrames);
						}
						m_mixEffectBlock2.PerformAutoTransition();
					}
					break;
			}
		}
		public void FTB(int me = 1)
		{
			switch (me)
			{
				case 1:
					if (m_mixEffectBlock1 != null)
					{
						m_mixEffectBlock1.PerformFadeToBlack();
					}
					break;
				case 2:
					if (m_mixEffectBlock2 != null)
					{
						m_mixEffectBlock2.PerformFadeToBlack();
					}

					break;
			}
		}
		public class Transition
		{
			private long InputId;
			public int me { get; set; }

		}
	}
}