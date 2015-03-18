using UnityEngine;
using System.Collections.Generic;
using System;

public class OnNextUpdater
{
	private class OnNextUpdaterEntry
	{
		public System.Action callback;
	}
	
	private List<OnNextUpdaterEntry> onNextUpdateEntries = new List<OnNextUpdaterEntry>();
	private List<OnNextUpdaterEntry> entriesToBeAdded = new List<OnNextUpdaterEntry>();
	private System.Object _lock = new System.Object();
	
	public void CallOnNextUpdate(System.Action callback)
	{
		lock (_lock) {
			entriesToBeAdded.Add(new OnNextUpdaterEntry() {
				callback = callback
			});
		}
	}
	
	public void Clear()
	{
		lock (_lock) {
			entriesToBeAdded.Clear();
			onNextUpdateEntries.Clear();
		}
	}
	
	public void UpdateTick()
	{
		lock (_lock) {
			for (int i = 0; i < entriesToBeAdded.Count; i++)
			{
				onNextUpdateEntries.Add(entriesToBeAdded[i]);
			}
			entriesToBeAdded.Clear();
			for (int i = 0; i < onNextUpdateEntries.Count; i++)
			{
				OnNextUpdaterEntry itr = onNextUpdateEntries[i];
				try {
					itr.callback();
				} catch (Exception e) {

					Debug.LogError(string.Format("ERROR {1}--{2}",e,e.Message,e.StackTrace));
				}
			}
			onNextUpdateEntries.Clear();
		}
	}
}