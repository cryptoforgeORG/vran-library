// ------------------------------------------------------------------------------
//  <autogenerated>
//      This code was generated by a tool.
//      Mono Runtime Version: 4.0.30319.1
// 
//      Changes to this file may cause incorrect behavior and will be lost if 
//      the code is regenerated.
//  </autogenerated>
// ------------------------------------------------------------------------------
using UnityEngine;
//using SimpleJSON;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VRAN
{
	public class SDK: MonoBehaviour
	{
		public int c;

		public string authorizeDeveloperEndpoint = "http://api.virtualrealityadnetwork.com:8888/authorize_developer";
		public string connectEndpoint = "http://api.virtualrealityadnetwork.com:8888/connect";


		public string adDataEndpoint = "http://api.virtualrealityadnetwork.com:8888/get_ad_data";
		public string storeAdAnalyticEndpoint = "http://api.virtualrealityadnetwork.com:8888/store_ad_analytic";

		public string ad_key = "7Ox5Wz1CeIQEnQ7VZP2i1xwgmMaLDk8g";
		public string app_key = "c1f8ee1b3fe2bfec4f61f88bb87e267c7602307f6a517c1f86c1fc2b412ad0768951af346c72001c";
		public string developer_key = "6f24855cf2a091bb13d377241559b2feb760ec287ef450252f2455d44e40ae0453d18aada4fab9b6";

		public string adId = "";
		public string appId = "";
		public string assetBundleEndpoint = "";
		public string assetBundleObject = "";

		public Vector3 adLocation;
		public Quaternion adRotation;

		public bool showAsset = true;

		protected bool isDeveloperAuthorized = false;

		public string session;

		public float start = 0;
		public float stop = 0;
		private float delta = 0;

		private GameObject prefab;

		private bool isSessionActive = false;

		private string version = "1.0";

		public void Ping() { 
//			Debug.Log ("_enter");
			Debug.Log ("developer_key: " + developer_key);
			Debug.Log ("app_key: " + app_key);
			Debug.Log ("ad_key: " + ad_key);
			Debug.Log ("adId: " + adId);
		}

		public string Version() { 
//			Debug.Log ("_enter");
			Debug.Log ("version: " + version);
			return version;
		}

		public void Update() {
			if (isSessionActive) {
				stop = Time.realtimeSinceStartup;
			}
		}

		#region TIMER METHODS
		public void StartSession() { 
//			Debug.Log ("_debug");
			//		StartCoroutine(PullAdDataCoroutine(adId));
			delta = 0;
			start = Time.realtimeSinceStartup;
			isSessionActive = true;
		}
		public void StopSession() { 
//			Debug.Log ("_debug");
			if (isDeveloperAuthorized) {
				delta = stop - start;
				StoreAdAnalytic();
			} else {
				Debug.Log ("Developer not is authorized VRAN");
			}
			isSessionActive = false;
		}
		#endregion
		#region CONFIGURE
		public void Configure(Vector3 location, Quaternion rotation, bool shouldShowAsset) { 
//			Debug.Log ("_enter");
			adLocation = location;
			adRotation = rotation;
			showAsset = shouldShowAsset;
		}
		#endregion
		#region CONNECT
		public void Connect(string developerKey, string appKey, string adKey) { 
//			Debug.Log ("_enter");
			StartCoroutine(ConnectCoroutine(developerKey, appKey, adKey));
		}
		IEnumerator ConnectCoroutine(string developerKey, string appKey, string adKey) { 
//			Debug.Log ("_enter");
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
//				print("Add data retrieved");
//				print (w.text);
//				print (w);
				
//				var response = SimpleJSON.JSON.Parse(w.text);
				var response = JSON.Parse(w.text);

//				print(response);
				
				bool error = response["error"].AsBool;
				
				if (error) {
//					Debug.Log ("+++++++");
					Debug.Log ("Developer not authorized to use VRAN");
//					Debug.Log ("+++++++");

				} else {
//					print (response["data"]);
					
					var data = response["data"];

//					Debug.Log ("+++++++");
//					Debug.Log ("Developer authorized");
//					Debug.Log ("+++++++");

//					print (data);
					
					var ad = data[0];

					
//					Debug.Log (ad);
					
					session = w.text;
					
					isDeveloperAuthorized = true;
					adId = ad["objectId"].Value;
					appId = ad["appId"].Value;

					PullAdData(adId);
				}
			}
		}
		#endregion
		#region PULLADDATA
		private void PullAdData(string adId) { 
//			Debug.Log ("_debug");
			StartCoroutine(PullAdDataCoroutine(adId));
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
//				print("Add data retrieved");
//				print (w.text);
//				print (w);
				
//				var response = SimpleJSON.JSON.Parse(w.text);
				var response = JSON.Parse(w.text);

//				print(response);
				
				bool error = response["error"].AsBool;
				
				if (error) {;	
					print (response["message"].Value);
				} else {
					
//					print (response["data"]);
//					print (response["data"].AsArray[0]);
					
					var ad = response["data"].AsArray[0];			
					
					assetBundleEndpoint = ad["s3_asset_bundle_path"].Value;
//					print (assetBundleEndpoint);
					
					assetBundleObject = ad["s3_asset_bundle_object"].Value;
//					print (ad["s3_asset_bundle_object"].Value);
					
					session = w.text;

					if (showAsset) {
						DownloadAsset();
					}
				}
			}
		}
		#endregion
		#region DOWNLOAD
		private void DownloadAsset() { 
//			Debug.Log ("_debug");
			StartCoroutine(DownloadCoroutine());
		}
		IEnumerator DownloadCoroutine () {
//			Debug.Log ("_enter");
			// Start a download of the given URL
			
//			Debug.Log (assetBundleEndpoint);
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
			prefab = request.asset as GameObject;
			
			GameObject adObject = Instantiate(prefab, adLocation, Quaternion.identity) as GameObject;

			adObject.name = "VRAN-Cylinder";
			adObject.AddComponent<VisibilityBehaviour>();

			// Unload the AssetBundles compressed contents to conserve memory
			bundle.Unload(false);
			
			// Frees the memory from the web stream
			www.Dispose();
		}
		#endregion
		#region STORE_AD_ANALYTIC
		private void StoreAdAnalytic() { 
//			Debug.Log ("_debug");
			StartCoroutine(StoreAdAnalyticCoroutine(adId, delta));
		}
		IEnumerator StoreAdAnalyticCoroutine(string adId, float seconds) {
			// We should only read the screen after all rendering is complete
			yield return new WaitForEndOfFrame();
			
			WWWForm form = new WWWForm();
			form.AddField("ad_id", adId);
			form.AddField("seconds", seconds.ToString());
			form.AddField("app_id", appId);
			
			WWW w = new WWW(storeAdAnalyticEndpoint, form);
			yield return w;
			if (!string.IsNullOrEmpty(w.error)) {
				print(w.error);
			} else {
//				print("Add data retrieved");
//				print (w.text);
//				print (w);
				
				//var response = SimpleJSON.JSON.Parse(w.text);
				var response = JSON.Parse(w.text);

//				print(response);
				
				bool error = response["error"].AsBool;
				
				if (error) {
					print(response["message"].Value);
				} else {			
//					print("Ad Analytic store");
					session = w.text;
				}
			}
		}
		#endregion
	}
	public class VisibilityBehaviour : MonoBehaviour {
		
		private Renderer objectRenderer;
		private SDK sdk;
		// Use this for initialization
		void Start () {
			objectRenderer = GetComponent<Renderer>();
			sdk = GameObject.Find("VRANClient").GetComponent<SDK>();
		}
		
		// Update is called once per frame

		private bool isSessionActive = false;
		private bool hasStoredAnalytic = false;

		void Update () {
			if (objectRenderer.isVisible) {
//				Debug.Log ("renderer.isVisible");
//				sdk.Ping();
				if (!isSessionActive) {
					sdk.StartSession();
					isSessionActive = true;
					hasStoredAnalytic = false;
				}
			} else {
				isSessionActive = false;

				if (!hasStoredAnalytic) {
					sdk.StopSession();
					hasStoredAnalytic = true;
				}
			}
		}
	}

	#region SIMPLEJSON
	public enum JSONBinaryTag
	{
		Array            = 1,
		Class            = 2,
		Value            = 3,
		IntValue        = 4,
		DoubleValue        = 5,
		BoolValue        = 6,
		FloatValue        = 7,
	}
	
	public class JSONNode
	{
		#region common interface
		public virtual void Add(string aKey, JSONNode aItem){ }
		public virtual JSONNode this[int aIndex]   { get { return null; } set { } }
		public virtual JSONNode this[string aKey]  { get { return null; } set { } }
		public virtual string Value                { get { return "";   } set { } }
		public virtual int Count                   { get { return 0;    } }
		
		public virtual void Add(JSONNode aItem)
		{
			Add("", aItem);
		}
		
		public virtual JSONNode Remove(string aKey) { return null; }
		public virtual JSONNode Remove(int aIndex) { return null; }
		public virtual JSONNode Remove(JSONNode aNode) { return aNode; }
		
		public virtual IEnumerable<JSONNode> Childs { get { yield break;} }
		public IEnumerable<JSONNode> DeepChilds
		{
			get
			{
				foreach (var C in Childs)
					foreach (var D in C.DeepChilds)
						yield return D;
			}
		}
		
		public override string ToString()
		{
			return "JSONNode";
		}
		public virtual string ToString(string aPrefix)
		{
			return "JSONNode";
		}
		
		#endregion common interface
		
		#region typecasting properties
		public virtual int AsInt
		{
			get
			{
				int v = 0;
				if (int.TryParse(Value,out v))
					return v;
				return 0;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual float AsFloat
		{
			get
			{
				float v = 0.0f;
				if (float.TryParse(Value,out v))
					return v;
				return 0.0f;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual double AsDouble
		{
			get
			{
				double v = 0.0;
				if (double.TryParse(Value,out v))
					return v;
				return 0.0;
			}
			set
			{
				Value = value.ToString();
			}
		}
		public virtual bool AsBool
		{
			get
			{
				bool v = false;
				if (bool.TryParse(Value,out v))
					return v;
				return !string.IsNullOrEmpty(Value);
			}
			set
			{
				Value = (value)?"true":"false";
			}
		}
		public virtual JSONArray AsArray
		{
			get
			{
				return this as JSONArray;
			}
		}
		public virtual JSONClass AsObject
		{
			get
			{
				return this as JSONClass;
			}
		}
		
		
		#endregion typecasting properties
		
		#region operators
		public static implicit operator JSONNode(string s)
		{
			return new JSONData(s);
		}
		public static implicit operator string(JSONNode d)
		{
			return (d == null)?null:d.Value;
		}
		public static bool operator ==(JSONNode a, object b)
		{
			if (b == null && a is JSONLazyCreator)
				return true;
			return System.Object.ReferenceEquals(a,b);
		}
		
		public static bool operator !=(JSONNode a, object b)
		{
			return !(a == b);
		}
		public override bool Equals (object obj)
		{
			return System.Object.ReferenceEquals(this, obj);
		}
		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}
		
		
		#endregion operators
		
		internal static string Escape(string aText)
		{
			string result = "";
			foreach(char c in aText)
			{
				switch(c)
				{
				case '\\' : result += "\\\\"; break;
				case '\"' : result += "\\\""; break;
				case '\n' : result += "\\n" ; break;
				case '\r' : result += "\\r" ; break;
				case '\t' : result += "\\t" ; break;
				case '\b' : result += "\\b" ; break;
				case '\f' : result += "\\f" ; break;
				default   : result += c     ; break;
				}
			}
			return result;
		}
		
		public static JSONNode Parse(string aJSON)
		{
			Stack<JSONNode> stack = new Stack<JSONNode>();
			JSONNode ctx = null;
			int i = 0;
			string Token = "";
			string TokenName = "";
			bool QuoteMode = false;
			while (i < aJSON.Length)
			{
				switch (aJSON[i])
				{
				case '{':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					stack.Push(new JSONClass());
					if (ctx != null)
					{
						TokenName = TokenName.Trim();
						if (ctx is JSONArray)
							ctx.Add(stack.Peek());
						else if (TokenName != "")
							ctx.Add(TokenName,stack.Peek());
					}
					TokenName = "";
					Token = "";
					ctx = stack.Peek();
					break;
					
				case '[':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					
					stack.Push(new JSONArray());
					if (ctx != null)
					{
						TokenName = TokenName.Trim();
						if (ctx is JSONArray)
							ctx.Add(stack.Peek());
						else if (TokenName != "")
							ctx.Add(TokenName,stack.Peek());
					}
					TokenName = "";
					Token = "";
					ctx = stack.Peek();
					break;
					
				case '}':
				case ']':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					if (stack.Count == 0)
						throw new Exception("JSON Parse: Too many closing brackets");
					
					stack.Pop();
					if (Token != "")
					{
						TokenName = TokenName.Trim();
						if (ctx is JSONArray)
							ctx.Add(Token);
						else if (TokenName != "")
							ctx.Add(TokenName,Token);
					}
					TokenName = "";
					Token = "";
					if (stack.Count>0)
						ctx = stack.Peek();
					break;
					
				case ':':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					TokenName = Token;
					Token = "";
					break;
					
				case '"':
					QuoteMode ^= true;
					break;
					
				case ',':
					if (QuoteMode)
					{
						Token += aJSON[i];
						break;
					}
					if (Token != "")
					{
						if (ctx is JSONArray)
							ctx.Add(Token);
						else if (TokenName != "")
							ctx.Add(TokenName, Token);
					}
					TokenName = "";
					Token = "";
					break;
					
				case '\r':
				case '\n':
					break;
					
				case ' ':
				case '\t':
					if (QuoteMode)
						Token += aJSON[i];
					break;
					
				case '\\':
					++i;
					if (QuoteMode)
					{
						char C = aJSON[i];
						switch (C)
						{
						case 't' : Token += '\t'; break;
						case 'r' : Token += '\r'; break;
						case 'n' : Token += '\n'; break;
						case 'b' : Token += '\b'; break;
						case 'f' : Token += '\f'; break;
						case 'u':
						{
							string s = aJSON.Substring(i+1,4);
							Token += (char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier);
							i += 4;
							break;
						}
						default  : Token += C; break;
						}
					}
					break;
					
				default:
					Token += aJSON[i];
					break;
				}
				++i;
			}
			if (QuoteMode)
			{
				throw new Exception("JSON Parse: Quotation marks seems to be messed up.");
			}
			return ctx;
		}
		
		public virtual void Serialize(System.IO.BinaryWriter aWriter) {}
		
		public void SaveToStream(System.IO.Stream aData)
		{
			var W = new System.IO.BinaryWriter(aData);
			Serialize(W);
		}
		
		#if USE_SharpZipLib
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
			{
				gzipOut.IsStreamOwner = false;
				SaveToStream(gzipOut);
				gzipOut.Close();
			}
		}
		
		public void SaveToCompressedFile(string aFileName)
		{
			#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToCompressedStream(F);
			}
			#else
			throw new Exception("Can't use File IO stuff in webplayer");
			#endif
		}
		public string SaveToCompressedBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToCompressedStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
		
		#else
		public void SaveToCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public void SaveToCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public string SaveToCompressedBase64()
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		#endif
		
		public void SaveToFile(string aFileName)
		{
			#if USE_FileIO
			System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
			using(var F = System.IO.File.OpenWrite(aFileName))
			{
				SaveToStream(F);
			}
			#else
			throw new Exception("Can't use File IO stuff in webplayer");
			#endif
		}
		public string SaveToBase64()
		{
			using (var stream = new System.IO.MemoryStream())
			{
				SaveToStream(stream);
				stream.Position = 0;
				return System.Convert.ToBase64String(stream.ToArray());
			}
		}
		public static JSONNode Deserialize(System.IO.BinaryReader aReader)
		{
			JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
			switch(type)
			{
			case JSONBinaryTag.Array:
			{
				int count = aReader.ReadInt32();
				JSONArray tmp = new JSONArray();
				for(int i = 0; i < count; i++)
					tmp.Add(Deserialize(aReader));
				return tmp;
			}
			case JSONBinaryTag.Class:
			{
				int count = aReader.ReadInt32();                
				JSONClass tmp = new JSONClass();
				for(int i = 0; i < count; i++)
				{
					string key = aReader.ReadString();
					var val = Deserialize(aReader);
					tmp.Add(key, val);
				}
				return tmp;
			}
			case JSONBinaryTag.Value:
			{
				return new JSONData(aReader.ReadString());
			}
			case JSONBinaryTag.IntValue:
			{
				return new JSONData(aReader.ReadInt32());
			}
			case JSONBinaryTag.DoubleValue:
			{
				return new JSONData(aReader.ReadDouble());
			}
			case JSONBinaryTag.BoolValue:
			{
				return new JSONData(aReader.ReadBoolean());
			}
			case JSONBinaryTag.FloatValue:
			{
				return new JSONData(aReader.ReadSingle());
			}
				
			default:
			{
				throw new Exception("Error deserializing JSON. Unknown tag: " + type);
			}
			}
		}
		
		#if USE_SharpZipLib
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
			return LoadFromStream(zin);
		}
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromCompressedStream(F);
			}
			#else
			throw new Exception("Can't use File IO stuff in webplayer");
			#endif
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromCompressedStream(stream);
		}
		#else
		public static JSONNode LoadFromCompressedFile(string aFileName)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		public static JSONNode LoadFromCompressedBase64(string aBase64)
		{
			throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
		}
		#endif
		
		public static JSONNode LoadFromStream(System.IO.Stream aData)
		{
			using(var R = new System.IO.BinaryReader(aData))
			{
				return Deserialize(R);
			}
		}
		public static JSONNode LoadFromFile(string aFileName)
		{
			#if USE_FileIO
			using(var F = System.IO.File.OpenRead(aFileName))
			{
				return LoadFromStream(F);
			}
			#else
			throw new Exception("Can't use File IO stuff in webplayer");
			#endif
		}
		public static JSONNode LoadFromBase64(string aBase64)
		{
			var tmp = System.Convert.FromBase64String(aBase64);
			var stream = new System.IO.MemoryStream(tmp);
			stream.Position = 0;
			return LoadFromStream(stream);
		}
	} // End of JSONNode
	
	public class JSONArray : JSONNode, IEnumerable
	{
		private List<JSONNode> m_List = new List<JSONNode>();
		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex<0 || aIndex >= m_List.Count)
					return new JSONLazyCreator(this);
				return m_List[aIndex];
			}
			set
			{
				if (aIndex<0 || aIndex >= m_List.Count)
					m_List.Add(value);
				else
					m_List[aIndex] = value;
			}
		}
		public override JSONNode this[string aKey]
		{
			get{ return new JSONLazyCreator(this);}
			set{ m_List.Add(value); }
		}
		public override int Count
		{
			get { return m_List.Count; }
		}
		public override void Add(string aKey, JSONNode aItem)
		{
			m_List.Add(aItem);
		}
		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_List.Count)
				return null;
			JSONNode tmp = m_List[aIndex];
			m_List.RemoveAt(aIndex);
			return tmp;
		}
		public override JSONNode Remove(JSONNode aNode)
		{
			m_List.Remove(aNode);
			return aNode;
		}
		public override IEnumerable<JSONNode> Childs
		{
			get
			{
				foreach(JSONNode N in m_List)
					yield return N;
			}
		}
		public IEnumerator GetEnumerator()
		{
			foreach(JSONNode N in m_List)
				yield return N;
		}
		public override string ToString()
		{
			string result = "[ ";
			foreach (JSONNode N in m_List)
			{
				if (result.Length > 2)
					result += ", ";
				result += N.ToString();
			}
			result += " ]";
			return result;
		}
		public override string ToString(string aPrefix)
		{
			string result = "[ ";
			foreach (JSONNode N in m_List)
			{
				if (result.Length > 3)
					result += ", ";
				result += "\n" + aPrefix + "   ";                
				result += N.ToString(aPrefix+"   ");
			}
			result += "\n" + aPrefix + "]";
			return result;
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONBinaryTag.Array);
			aWriter.Write(m_List.Count);
			for(int i = 0; i < m_List.Count; i++)
			{
				m_List[i].Serialize(aWriter);
			}
		}
	} // End of JSONArray
	
	public class JSONClass : JSONNode, IEnumerable
	{
		private Dictionary<string,JSONNode> m_Dict = new Dictionary<string,JSONNode>();
		public override JSONNode this[string aKey]
		{
			get
			{
				if (m_Dict.ContainsKey(aKey))
					return m_Dict[aKey];
				else
					return new JSONLazyCreator(this, aKey);
			}
			set
			{
				if (m_Dict.ContainsKey(aKey))
					m_Dict[aKey] = value;
				else
					m_Dict.Add(aKey,value);
			}
		}
		public override JSONNode this[int aIndex]
		{
			get
			{
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return null;
				return m_Dict.ElementAt(aIndex).Value;
			}
			set
			{
				if (aIndex < 0 || aIndex >= m_Dict.Count)
					return;
				string key = m_Dict.ElementAt(aIndex).Key;
				m_Dict[key] = value;
			}
		}
		public override int Count
		{
			get { return m_Dict.Count; }
		}
		
		
		public override void Add(string aKey, JSONNode aItem)
		{
			if (!string.IsNullOrEmpty(aKey))
			{
				if (m_Dict.ContainsKey(aKey))
					m_Dict[aKey] = aItem;
				else
					m_Dict.Add(aKey, aItem);
			}
			else
				m_Dict.Add(Guid.NewGuid().ToString(), aItem);
		}
		
		public override JSONNode Remove(string aKey)
		{
			if (!m_Dict.ContainsKey(aKey))
				return null;
			JSONNode tmp = m_Dict[aKey];
			m_Dict.Remove(aKey);
			return tmp;        
		}
		public override JSONNode Remove(int aIndex)
		{
			if (aIndex < 0 || aIndex >= m_Dict.Count)
				return null;
			var item = m_Dict.ElementAt(aIndex);
			m_Dict.Remove(item.Key);
			return item.Value;
		}
		public override JSONNode Remove(JSONNode aNode)
		{
			try
			{
				var item = m_Dict.Where(k => k.Value == aNode).First();
				m_Dict.Remove(item.Key);
				return aNode;
			}
			catch
			{
				return null;
			}
		}
		
		public override IEnumerable<JSONNode> Childs
		{
			get
			{
				foreach(KeyValuePair<string,JSONNode> N in m_Dict)
					yield return N.Value;
			}
		}
		
		public IEnumerator GetEnumerator()
		{
			foreach(KeyValuePair<string, JSONNode> N in m_Dict)
				yield return N;
		}
		public override string ToString()
		{
			string result = "{";
			foreach (KeyValuePair<string, JSONNode> N in m_Dict)
			{
				if (result.Length > 2)
					result += ", ";
				result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
			}
			result += "}";
			return result;
		}
		public override string ToString(string aPrefix)
		{
			string result = "{ ";
			foreach (KeyValuePair<string, JSONNode> N in m_Dict)
			{
				if (result.Length > 3)
					result += ", ";
				result += "\n" + aPrefix + "   ";
				result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix+"   ");
			}
			result += "\n" + aPrefix + "}";
			return result;
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			aWriter.Write((byte)JSONBinaryTag.Class);
			aWriter.Write(m_Dict.Count);
			foreach(string K in m_Dict.Keys)
			{
				aWriter.Write(K);
				m_Dict[K].Serialize(aWriter);
			}
		}
	} // End of JSONClass
	
	public class JSONData : JSONNode
	{
		private string m_Data;
		public override string Value
		{
			get { return m_Data; }
			set { m_Data = value; }
		}
		public JSONData(string aData)
		{
			m_Data = aData;
		}
		public JSONData(float aData)
		{
			AsFloat = aData;
		}
		public JSONData(double aData)
		{
			AsDouble = aData;
		}
		public JSONData(bool aData)
		{
			AsBool = aData;
		}
		public JSONData(int aData)
		{
			AsInt = aData;
		}
		
		public override string ToString()
		{
			return "\"" + Escape(m_Data) + "\"";
		}
		public override string ToString(string aPrefix)
		{
			return "\"" + Escape(m_Data) + "\"";
		}
		public override void Serialize (System.IO.BinaryWriter aWriter)
		{
			var tmp = new JSONData("");
			
			tmp.AsInt = AsInt;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.IntValue);
				aWriter.Write(AsInt);
				return;
			}
			tmp.AsFloat = AsFloat;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.FloatValue);
				aWriter.Write(AsFloat);
				return;
			}
			tmp.AsDouble = AsDouble;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.DoubleValue);
				aWriter.Write(AsDouble);
				return;
			}
			
			tmp.AsBool = AsBool;
			if (tmp.m_Data == this.m_Data)
			{
				aWriter.Write((byte)JSONBinaryTag.BoolValue);
				aWriter.Write(AsBool);
				return;
			}
			aWriter.Write((byte)JSONBinaryTag.Value);
			aWriter.Write(m_Data);
		}
	} // End of JSONData
	
	internal class JSONLazyCreator : JSONNode
	{
		private JSONNode m_Node = null;
		private string m_Key = null;
		
		public JSONLazyCreator(JSONNode aNode)
		{
			m_Node = aNode;
			m_Key  = null;
		}
		public JSONLazyCreator(JSONNode aNode, string aKey)
		{
			m_Node = aNode;
			m_Key = aKey;
		}
		
		private void Set(JSONNode aVal)
		{
			if (m_Key == null)
			{
				m_Node.Add(aVal);
			}
			else
			{
				m_Node.Add(m_Key, aVal);
			}
			m_Node = null; // Be GC friendly.
		}
		
		public override JSONNode this[int aIndex]
		{
			get
			{
				return new JSONLazyCreator(this);
			}
			set
			{
				var tmp = new JSONArray();
				tmp.Add(value);
				Set(tmp);
			}
		}
		
		public override JSONNode this[string aKey]
		{
			get
			{
				return new JSONLazyCreator(this, aKey);
			}
			set
			{
				var tmp = new JSONClass();
				tmp.Add(aKey, value);
				Set(tmp);
			}
		}
		public override void Add (JSONNode aItem)
		{
			var tmp = new JSONArray();
			tmp.Add(aItem);
			Set(tmp);
		}
		public override void Add (string aKey, JSONNode aItem)
		{
			var tmp = new JSONClass();
			tmp.Add(aKey, aItem);
			Set(tmp);
		}
		public static bool operator ==(JSONLazyCreator a, object b)
		{
			if (b == null)
				return true;
			return System.Object.ReferenceEquals(a,b);
		}
		
		public static bool operator !=(JSONLazyCreator a, object b)
		{
			return !(a == b);
		}
		public override bool Equals (object obj)
		{
			if (obj == null)
				return true;
			return System.Object.ReferenceEquals(this, obj);
		}
		public override int GetHashCode ()
		{
			return base.GetHashCode();
		}
		
		public override string ToString()
		{
			return "";
		}
		public override string ToString(string aPrefix)
		{
			return "";
		}
		
		public override int AsInt
		{
			get
			{
				JSONData tmp = new JSONData(0);
				Set(tmp);
				return 0;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override float AsFloat
		{
			get
			{
				JSONData tmp = new JSONData(0.0f);
				Set(tmp);
				return 0.0f;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override double AsDouble
		{
			get
			{
				JSONData tmp = new JSONData(0.0);
				Set(tmp);
				return 0.0;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override bool AsBool
		{
			get
			{
				JSONData tmp = new JSONData(false);
				Set(tmp);
				return false;
			}
			set
			{
				JSONData tmp = new JSONData(value);
				Set(tmp);
			}
		}
		public override JSONArray AsArray
		{
			get
			{
				JSONArray tmp = new JSONArray();
				Set(tmp);
				return tmp;
			}
		}
		public override JSONClass AsObject
		{
			get
			{
				JSONClass tmp = new JSONClass();
				Set(tmp);
				return tmp;
			}
		}
	} // End of JSONLazyCreator
	
	public static class JSON
	{
		public static JSONNode Parse(string aJSON)
		{
			return JSONNode.Parse(aJSON);
		}
	}
	#endregion
}