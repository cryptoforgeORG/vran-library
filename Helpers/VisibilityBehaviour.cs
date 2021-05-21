using UnityEngine;
using System.Collections;

namespace VRAN
{
	public class VisibilityBehaviour : MonoBehaviour {
		
		private Renderer objectRenderer;
		// Use this for initialization
		void Start () {
			objectRenderer = GetComponent<Renderer>();
		}
		
		// Update is called once per frame
		void Update () {
			if (objectRenderer.isVisible) {
				Debug.Log ("renderer.isVisible");
			}
		}
	}
}