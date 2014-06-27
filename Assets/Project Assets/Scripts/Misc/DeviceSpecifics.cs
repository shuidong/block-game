using UnityEngine;
using System.Collections;

public class DeviceSpecifics : MonoBehaviour {

	public Object[] desktopOnly;
	public Object[] mobileOnly;

	void Start () {
		RuntimePlatform pl = Application.platform;

		if (pl == RuntimePlatform.Android || pl == RuntimePlatform.IPhonePlayer) {
			foreach (Object o in desktopOnly) {
				Destroy(o);
			}
		} else {
			foreach (Object o in mobileOnly) {
				Destroy(o);
			}
		}
	}
}
