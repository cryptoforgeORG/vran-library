using UnityEngine;
using UnityEngine.UI;
using SimpleJSON;
using System.Collections;
using VRAN;

public class APIClient: MonoBehaviour {

	public string adDataEndpoint = "http://52.2.23.36:8888/get_add_data";
	public string authorizeDeveloperEndpoint = "http://52.2.23.36:8888/authorize_developer";
	public string storeAdAnalyticEndpoint = "http://52.2.23.36:8888/store_ad_analytic";
	public string connectEndpoint = "http://52.2.23.36:8888/connect";

	public string assetBundleEndpoint = "";
	public string assetBundleObject = "";
	
	public string ad_key = "7Ox5Wz1CeIQEnQ7VZP2i1xwgmMaLDk8g";
	public string app_key = "c1f8ee1b3fe2bfec4f61f88bb87e267c7602307f6a517c1f86c1fc2b412ad0768951af346c72001c";
	public string developer_key = "6f24855cf2a091bb13d377241559b2feb760ec287ef450252f2455d44e40ae0453d18aada4fab9b6";

	public string adId = "";
	public string developerId = "";
	public string apiKey = "";

	public string session;

	public Button AuthorizeDeveloperButton = null;
	public Button LoadAdDataButton = null;
	public Button LoadAdAssetButton = null;
	public Button StartSessionButton = null;
	public Button StopSessionButton = null;
	
	protected bool isDeveloperAuthorized = false;
	
	void Awake() {
		GameObject.Find("S3AssetUrl").GetComponent<Text>().text = "";
		GameObject.Find("S3AssetBundleObject").GetComponent<Text>().text = "";				

//		AuthorizeDeveloper(developerId, apiKey);
		Connect(developer_key, app_key, ad_key);

		GameObject.Find("DeveloperKeyText").GetComponent<Text>().text = string.Format("dev_key: {0}", developer_key);
		GameObject.Find("DeveloperKeyText").GetComponent<Text>().color = Color.cyan;

		GameObject.Find("AppKeyText").GetComponent<Text>().text = string.Format("app_key: {0}", app_key);
		GameObject.Find("AppKeyText").GetComponent<Text>().color = Color.cyan;

		GameObject.Find("AdKeyText").GetComponent<Text>().text = string.Format("ad_key: {0}", ad_key);
		GameObject.Find("AdKeyText").GetComponent<Text>().color = Color.cyan;

		DontDestroyOnLoad(this.gameObject);

//		 VRAN LIBRARY

		SDK sdk = new SDK();
		sdk.AuthorizeDeveloper();
	}
	void Start () {
		AuthorizeDeveloperButton.onClick.AddListener(() => { AuthorizeDeveloper(developerId, apiKey); });
		LoadAdDataButton.onClick.AddListener(() => { PullAdData(adId); });
		LoadAdAssetButton.onClick.AddListener(() => { LoadAdAsset(); });
		StartSessionButton.onClick.AddListener(() => { StartSession(); });
		StopSessionButton.onClick.AddListener(() => { StopSession(); });
	}

	float start = 0;
	float stop = 0;
	float delta = 0;

	bool measureWatchTime = false;

	void Update () {
		if (measureWatchTime) {
			delta = Time.realtimeSinceStartup - start;
		}
		GameObject.Find("TimeSpentLooking").GetComponent<Text>().text = string.Format ("{0}", delta);

	}
	
	public void AuthorizeDeveloper(string developerId, string apiKey) { Debug.Log ("_debug");
		StartCoroutine(AuthorizeDeveloperCoroutine(developerId, apiKey));
	}
	public void PullAdData(string adId) { Debug.Log ("_debug");
		StartCoroutine(PullAdDataCoroutine(adId));
	}
	public void LoadAdAsset() { Debug.Log ("_debug");
		StartCoroutine(DownloadCoroutine());
	}
	public void StartSession() { Debug.Log ("_debug");
		//		StartCoroutine(PullAdDataCoroutine(adId));
		delta = 0;
		start = Time.realtimeSinceStartup;
		measureWatchTime = true;
	}
	public void StopSession() { Debug.Log ("_debug");
		//		StartCoroutine(PullAdDataCoroutine(adId));

		if (isDeveloperAuthorized) {
			stop = Time.realtimeSinceStartup;
			delta = stop - start;
			measureWatchTime = false;
			StartCoroutine(StoreAdAnalytic(adId, delta));
		} else {
			GameObject.Find("ConsoleMessage").GetComponent<Text>().text = "Developer not is authorized";
			GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.red;	
		}
	}
	public void DownloadAssetBundle() { Debug.Log ("_debug");
		StartCoroutine(DownloadCoroutine());
	}
	
	#region CONNECT
	public void Connect(string developerKey, string appKey, string adKey) { Debug.Log ("_enter");
		StartCoroutine(ConnectCoroutine(developerKey, appKey, adKey));
	}
	IEnumerator ConnectCoroutine(string developerKey, string appKey, string adKey) { Debug.Log ("_enter");
		// We should only read the screen after all rendering is complete
		yield return new WaitForEndOfFrame();
		
		WWWForm form = new WWWForm();
		form.AddField("developer_key", developerKey);
		form.AddField("app_key", appKey);
		form.AddField("ad_keys", "XXXXX");
		form.AddField("ad_keys", adKey);

		WWW w = new WWW(connectEndpoint, form);
		yield return w;
		
		if (!string.IsNullOrEmpty(w.error)) {
			print(w.error);
		} else {
			print("Add data retrieved");
			print (w.text);
			print (w);
			
			var response = SimpleJSON.JSON.Parse(w.text);
			
			print(response);
			
			bool error = response["error"].AsBool;
			
			if (error) {
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = response["message"].Value;
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.red;						
			} else {
				print (response["data"]);
				
				var data = response["data"];
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = "Developer is authorized";
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.green;				

				print (data);

				var ad = data[0]["objectId"];

				Debug.Log (ad);

				session = w.text;
				
				isDeveloperAuthorized = true;
				adId = ad.Value;
			}
		}
	}
	#endregion
	IEnumerator AuthorizeDeveloperCoroutine(string developerId, string apiKey) {
		// We should only read the screen after all rendering is complete
		yield return new WaitForEndOfFrame();
		
		WWWForm form = new WWWForm();
		form.AddField("developer_id", developerId);
		form.AddField("api_key", apiKey);

		WWW w = new WWW(authorizeDeveloperEndpoint, form);
		yield return w;

		if (!string.IsNullOrEmpty(w.error)) {
			print(w.error);
		} else {
			print("Add data retrieved");
			print (w.text);
			print (w);
			
			var response = SimpleJSON.JSON.Parse(w.text);
			
			print(response);
			
			bool error = response["error"].AsBool;
			
			if (error) {
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = response["message"].Value;
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.red;						
			} else {
				print (response["data"]);

				var developer = response["data"];
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = "Developer is authorized";
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.green;				
				
				session = w.text;

				isDeveloperAuthorized = true;
			}
		}
	}	
	IEnumerator PullAdDataCoroutine(string adId) {
		// We should only read the screen after all rendering is complete
		yield return new WaitForEndOfFrame();
		
		WWWForm form = new WWWForm();
		form.AddField("ad_id", adId);

		WWW w = new WWW(adDataEndpoint, form);
		yield return w;
		if (!string.IsNullOrEmpty(w.error)) {
			print(w.error);
		} else {
			print("Add data retrieved");
			print (w.text);
			print (w);
			
			var response = SimpleJSON.JSON.Parse(w.text);

			print(response);
			
			bool error = response["error"].AsBool;
			
			if (error) {
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = response["message"].Value;
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.red;				

			} else {

				print (response["data"]);
				print (response["data"].AsArray[0]);
								    
				var ad = response["data"].AsArray[0];
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = "Ad Data loaded";
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.green;			

				assetBundleEndpoint = ad["s3_asset_bundle_path"].Value;
				GameObject.Find("S3AssetUrl").GetComponent<Text>().text = ad["s3_asset_bundle_path"].Value;				
				GameObject.Find("S3AssetUrl").GetComponent<Text>().color = Color.green;	

				assetBundleObject = ad["s3_asset_bundle_object"].Value;
				GameObject.Find("S3AssetBundleObject").GetComponent<Text>().text = ad["s3_asset_bundle_object"].Value;				
				GameObject.Find("S3AssetBundleObject").GetComponent<Text>().color = Color.green;	
				
				session = w.text;
			}
		}
	}
	IEnumerator StoreAdAnalytic(string adId, float seconds) {
		// We should only read the screen after all rendering is complete
		yield return new WaitForEndOfFrame();
		
		WWWForm form = new WWWForm();
		form.AddField("ad_id", adId);
		form.AddField("seconds", seconds.ToString());
		
		WWW w = new WWW(storeAdAnalyticEndpoint, form);
		yield return w;
		if (!string.IsNullOrEmpty(w.error)) {
			print(w.error);
		} else {
			print("Add data retrieved");
			print (w.text);
			print (w);
			
			var response = SimpleJSON.JSON.Parse(w.text);
			
			print(response);
			
			bool error = response["error"].AsBool;
			
			if (error) {
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = response["message"].Value;
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.red;				
				
			} else {
				
				GameObject.Find("ConsoleMessage").GetComponent<Text>().text = "Ad Analytic stored";
				GameObject.Find("ConsoleMessage").GetComponent<Text>().color = Color.green;			
				
				session = w.text;
				
				//				adId =
			}
		}
	}
	IEnumerator DownloadCoroutine () { Debug.Log ("_enter");
		// Start a download of the given URL
		
		Debug.Log (assetBundleEndpoint);
		WWW www = WWW.LoadFromCacheOrDownload (assetBundleEndpoint, 1);
		
		// Wait for download to complete
		yield return www;
		
		// Load and retrieve the AssetBundle
		AssetBundle bundle = www.assetBundle;
		
		// Load the object asynchronously
		AssetBundleRequest request = bundle.LoadAssetAsync (assetBundleObject, typeof(GameObject));
		
		// Wait for completion
		yield return request;
		
		// Get the reference to the loaded object
		GameObject prefab = request.asset as GameObject;
		
		Instantiate(prefab, new Vector3(0, 0, 0), Quaternion.identity);
		
		// Unload the AssetBundles compressed contents to conserve memory
		bundle.Unload(false);
		
		// Frees the memory from the web stream
		www.Dispose();
	}
}
