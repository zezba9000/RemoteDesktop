using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

namespace WinSound
{
	/// <summary>
	/// QueueTimer
	/// </summary>
	public class QueueTimer
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public QueueTimer()
		{
			m_DelegateTimerProc = new global::WinSound.Win32.DelegateTimerProc(OnTimer);
		}

		//Attribute
		private bool m_IsRunning = false;
		private uint m_Milliseconds = 20;
		private IntPtr m_HandleTimer = IntPtr.Zero;
		private GCHandle m_GCHandleTimer;
		private uint m_ResolutionInMilliseconds = 0;
		private IntPtr m_HandleTimerQueue;
		private GCHandle m_GCHandleTimerQueue;

		//Delegates bzw. Events
		private global::WinSound.Win32.DelegateTimerProc m_DelegateTimerProc;
		public delegate void DelegateTimerTick();
		public event DelegateTimerTick TimerTick;

		/// <summary>
		/// IsRunning
		/// </summary>
		/// <returns></returns>
		public bool IsRunning
		{
			get
			{
				return m_IsRunning;
			}
		}
		/// <summary>
		/// Milliseconds
		/// </summary>
		public uint Milliseconds
		{
			get
			{
				return m_Milliseconds;
			}
		}
		/// <summary>
		/// ResolutionInMilliseconds
		/// </summary>
		public uint ResolutionInMilliseconds
		{
			get
			{
				return m_ResolutionInMilliseconds;
			}
		}
		/// <summary>
		/// SetBestResolution
		/// </summary>
		public static void SetBestResolution()
		{
			//QueueTimer Auflösung ermitteln
			global::WinSound.Win32.TimeCaps tc = new global::WinSound.Win32.TimeCaps();
			global::WinSound.Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(global::WinSound.Win32.TimeCaps)));
			uint resolution = Math.Max(tc.wPeriodMin, 0);

			//QueueTimer Resolution setzen
			global::WinSound.Win32.TimeBeginPeriod(resolution);
		}
		/// <summary>
		/// ResetResolution
		/// </summary>
		public static void ResetResolution()
		{
			//QueueTimer Auflösung ermitteln
			global::WinSound.Win32.TimeCaps tc = new global::WinSound.Win32.TimeCaps();
			global::WinSound.Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(global::WinSound.Win32.TimeCaps)));
			uint resolution = Math.Max(tc.wPeriodMin, 0);

			//QueueTimer Resolution deaktivieren
			global::WinSound.Win32.TimeBeginPeriod(resolution);
		}
		/// <summary>
		/// Start
		/// </summary>
		/// <param name="milliseconds"></param>
		/// <param name="dueTimeInMilliseconds"></param>
		public void Start(uint milliseconds, uint dueTimeInMilliseconds)
		{
			//Werte übernehmen
			m_Milliseconds = milliseconds;

			//QueueTimer Auflösung ermitteln
			global::WinSound.Win32.TimeCaps tc = new global::WinSound.Win32.TimeCaps();
			global::WinSound.Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(global::WinSound.Win32.TimeCaps)));
			m_ResolutionInMilliseconds = Math.Max(tc.wPeriodMin, 0);

			//QueueTimer Resolution setzen
			global::WinSound.Win32.TimeBeginPeriod(m_ResolutionInMilliseconds);

			//QueueTimer Queue erstellen
			m_HandleTimerQueue = global::WinSound.Win32.CreateTimerQueue();
			m_GCHandleTimerQueue = GCHandle.Alloc(m_HandleTimerQueue);

			//Versuche QueueTimer zu starten
			bool resultCreate = global::WinSound.Win32.CreateTimerQueueTimer(out m_HandleTimer, m_HandleTimerQueue, m_DelegateTimerProc, IntPtr.Zero, dueTimeInMilliseconds, m_Milliseconds, global::WinSound.Win32.WT_EXECUTEINTIMERTHREAD);
			if (resultCreate)
			{
				//Handle im Speicher halten
				m_GCHandleTimer = GCHandle.Alloc(m_HandleTimer, GCHandleType.Pinned);
				//QueueTimer ist gestartet
				m_IsRunning = true;
			}
		}
		/// <summary>
		/// Stop
		/// </summary>
		public void Stop()
		{
			if (m_HandleTimer != IntPtr.Zero)
			{
				//QueueTimer beenden
				global:: WinSound.Win32.DeleteTimerQueueTimer(IntPtr.Zero, m_HandleTimer, IntPtr.Zero);
				//QueueTimer Resolution beenden
				global::WinSound.Win32.TimeEndPeriod(m_ResolutionInMilliseconds);

				//QueueTimer Queue löschen
				if (m_HandleTimerQueue != IntPtr.Zero)
				{
					global::WinSound.Win32.DeleteTimerQueue(m_HandleTimerQueue);
				}

				//Handles freigeben
				if (m_GCHandleTimer.IsAllocated)
				{
					m_GCHandleTimer.Free();
				}
				if (m_GCHandleTimerQueue.IsAllocated)
				{
					m_GCHandleTimerQueue.Free();
				}

				//Variablen setzen
				m_HandleTimer = IntPtr.Zero;
				m_HandleTimerQueue = IntPtr.Zero;
				m_IsRunning = false;
			}
		}
		/// <summary>
		/// OnTimer
		/// </summary>
		/// <param name="lpParameter"></param>
		/// <param name="TimerOrWaitFired"></param>
		private void OnTimer(IntPtr lpParameter, bool TimerOrWaitFired)
		{
			if (TimerTick != null)
			{
				TimerTick();
			}
		}
	}

	/// <summary>
	/// QueueTimer
	/// </summary>
	public class EventTimer
	{
		/// <summary>
		/// Konstruktor
		/// </summary>
		public EventTimer()
		{
			m_DelegateTimeEvent = new global::WinSound.Win32.TimerEventHandler(OnTimer);
		}

		//Attribute
		private bool m_IsRunning = false;
		private uint m_Milliseconds = 20;
		private UInt32 m_TimerId = 0;
		private GCHandle m_GCHandleTimer;
		private UInt32 m_UserData = 0;
		private uint m_ResolutionInMilliseconds = 0;

		//Delegates bzw. Events
		private global::WinSound.Win32.TimerEventHandler m_DelegateTimeEvent;
		public delegate void DelegateTimerTick();
		public event DelegateTimerTick TimerTick;

		/// <summary>
		/// IsRunning
		/// </summary>
		/// <returns></returns>
		public bool IsRunning
		{
			get
			{
				return m_IsRunning;
			}
		}
		/// <summary>
		/// Milliseconds
		/// </summary>
		public uint Milliseconds
		{
			get
			{
				return m_Milliseconds;
			}
		}
		/// <summary>
		/// ResolutionInMilliseconds
		/// </summary>
		public uint ResolutionInMilliseconds
		{
			get
			{
				return m_ResolutionInMilliseconds;
			}
		}
		/// <summary>
		/// SetBestResolution
		/// </summary>
		public static void SetBestResolution()
		{
			//QueueTimer Auflösung ermitteln
			global::WinSound.Win32.TimeCaps tc = new global::WinSound.Win32.TimeCaps();
			global::WinSound.Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(global::WinSound.Win32.TimeCaps)));
			uint resolution = Math.Max(tc.wPeriodMin, 0);

			//QueueTimer Resolution setzen
			global::WinSound.Win32.TimeBeginPeriod(resolution);
		}
		/// <summary>
		/// ResetResolution
		/// </summary>
		public static void ResetResolution()
		{
			//QueueTimer Auflösung ermitteln
			global::WinSound.Win32.TimeCaps tc = new global::WinSound.Win32.TimeCaps();
			global::WinSound.Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(global::WinSound.Win32.TimeCaps)));
			uint resolution = Math.Max(tc.wPeriodMin, 0);

			//QueueTimer Resolution deaktivieren
			global::WinSound.Win32.TimeEndPeriod(resolution);
		}
		/// <summary>
		/// Start
		/// </summary>
		/// <param name="milliseconds"></param>
		/// <param name="dueTimeInMilliseconds"></param>
		public void Start(uint milliseconds, uint dueTimeInMilliseconds)
		{
			//Werte übernehmen
			m_Milliseconds = milliseconds;

			//Timer Auflösung ermitteln
			global::WinSound.Win32.TimeCaps tc = new global::WinSound.Win32.TimeCaps();
			global::WinSound.Win32.TimeGetDevCaps(ref tc, (uint)Marshal.SizeOf(typeof(global::WinSound.Win32.TimeCaps)));
			m_ResolutionInMilliseconds = Math.Max(tc.wPeriodMin, 0);

			//Timer Resolution setzen
			global::WinSound.Win32.TimeBeginPeriod(m_ResolutionInMilliseconds);

			//Versuche EventTimer zu starten
			m_TimerId = global::WinSound.Win32.TimeSetEvent(m_Milliseconds, m_ResolutionInMilliseconds, m_DelegateTimeEvent, ref m_UserData, (UInt32)Win32.TIME_PERIODIC);
			if (m_TimerId > 0)
			{
				//Handle im Speicher halten
				m_GCHandleTimer = GCHandle.Alloc(m_TimerId, GCHandleType.Pinned);
				//QueueTimer ist gestartet
				m_IsRunning = true;
			}
		}
		/// <summary>
		/// Stop
		/// </summary>
		public void Stop()
		{
			if (m_TimerId > 0)
			{
				//Timer beenden
				global:: WinSound.Win32.TimeKillEvent(m_TimerId);
				//Timer Resolution beenden
				global::WinSound.Win32.TimeEndPeriod(m_ResolutionInMilliseconds);

				//Handles freigeben
				if (m_GCHandleTimer.IsAllocated)
				{
					m_GCHandleTimer.Free();
				}

				//Variablen setzen
				m_TimerId = 0;
				m_IsRunning = false;
			}
		}
		/// <summary>
		/// OnTimer
		/// </summary>
		/// <param name="lpParameter"></param>
		/// <param name="TimerOrWaitFired"></param>
		private void OnTimer(UInt32 id, UInt32 msg, ref UInt32 userCtx, UInt32 rsv1, UInt32 rsv2)
		{
			if (TimerTick != null)
			{
				TimerTick();
			}
		}
	}
	/// <summary>
	/// Stopwatch
	/// </summary>
	public class Stopwatch
	{
		/// <summary>
		/// Stopwatch
		/// </summary>
		public Stopwatch()
		{
			//Prüfen
			if (Win32.QueryPerformanceFrequency(out m_Frequency) == false)
			{
				throw new Exception("High Performance counter not supported");
			}
		}

		//Attribute
		private long m_StartTime = 0;
		private long m_DurationTime = 0;
		private long m_Frequency;

		/// <summary>
		/// Start
		/// </summary>
		public void Start()
		{
			Win32.QueryPerformanceCounter(out m_StartTime);
			m_DurationTime = m_StartTime;
		}
		/// <summary>
		/// ElapsedMilliseconds
		/// </summary>
		public double ElapsedMilliseconds
		{
			get
			{
				Win32.QueryPerformanceCounter(out m_DurationTime);
				return (double)(m_DurationTime - m_StartTime) / (double)m_Frequency * 1000;
			}
		}
	}
}
