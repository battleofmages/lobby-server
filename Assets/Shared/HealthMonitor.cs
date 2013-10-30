/*
using UnityEngine;
using System.Collections;
using System.Diagnostics;

// Doesn't work for some reason
public static class HealthMonitor {
	private static PerformanceCounter cpuCounter;
	private static PerformanceCounter ramCounter;
	
	private static float lastCPUPoll;
	private static float lastRAMPoll;
	
	private static float _cpuUsage;
	private static float _freeRAM;
	
	// Init
	static void Init() {
		cpuCounter = new PerformanceCounter();
		
		cpuCounter.CategoryName = "Processor";
		cpuCounter.CounterName = "% Processor Time";
		cpuCounter.InstanceName = "_Total";
		cpuCounter.NextValue();
		
		ramCounter = new PerformanceCounter("Memory", "Available MBytes");
		ramCounter.NextValue();
	}
	
	// CPU usage in %
	public static float cpuUsage {
		get {
			if(Time.time - lastCPUPoll >= 1.0f) {
				_cpuUsage = cpuCounter.NextValue();
				lastCPUPoll = Time.time;
			}
			
			return _cpuUsage;
		}
	}
	
	// RAM usage in MB
	public static float freeRAM {
		get {
			if(Time.time - lastRAMPoll >= 1.0f) {
				_freeRAM = ramCounter.NextValue();
				lastRAMPoll = Time.time;
			}
			
			return _freeRAM;
		}
	}
}
*/