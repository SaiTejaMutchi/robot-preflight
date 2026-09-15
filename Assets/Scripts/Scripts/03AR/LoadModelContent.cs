
using UnityEngine;
using GLTFast;
using UnityEngine.SceneManagement;
public class LoadModelContent : MonoBehaviour
{
	GameObject model;
	public static LoadModelContent instanceModel;

	private Scene scene;

	private void Start()
	{
		scene = SceneManager.GetActiveScene();
		instanceModel = this;
	}

	public void GetModel_Info(string path, string modelID, Vector3 _position, Vector3 _rotation, Vector3 _scale)
	{
		Debug.Log("GetModel_Info");
		LoadModelFast(path, modelID, _position, _rotation, _scale);

	}

	async void LoadModelFast(string path, string modelID, Vector3 _position, Vector3 _rotation, Vector3 _scale)
	{


		Debug.Log("LoadModelFast");
		model = new GameObject();
		model.name = "DownloadeMesh";
		model.transform.localScale = Vector3.one;
		model.AddComponent<GltfAsset>();
		var success = await model.GetComponent<GltfAsset>().Load(path);

		if (success)
		{
			GameObject rootObject = new GameObject();
			rootObject.name = "MeshRootObject";
			rootObject.transform.SetParent(GameObject.Find("Tracking").transform);
			rootObject.transform.localPosition = Vector3.zero;
			rootObject.transform.localRotation = Quaternion.Euler(0, 0, 0);
			rootObject.transform.localScale = new Vector3(1f, 1f, 1f);

			model.transform.SetParent(rootObject.transform);

			_rotation.y = -_rotation.y;
			_rotation.z = -_rotation.z;

			Quaternion qZ = Quaternion.Euler(0, 0, _rotation.z);
			Quaternion qX = Quaternion.Euler(_rotation.x, 0, 0);
			Quaternion qY = Quaternion.Euler(0, _rotation.y, 0);
			Quaternion finalRotation = qZ * qX * qY;

			if (scene.name == "05Vuforia")
			{
				// model.transform.localPosition = new Vector3(-_position.x, _position.y, _position.z);
			}
			else
			{

				Debug.Log(_position + "   aa......");
				model.transform.localPosition = new Vector3(_position.x, _position.y, _position.z);

			}
			model.transform.localRotation = finalRotation;
			model.transform.localScale = new Vector3(_scale.x, _scale.y, _scale.z);


			foreach (Renderer renderer in model.GetComponentsInChildren<Renderer>())
			{
				if (model.transform.GetComponentsInChildren<SkinnedMeshRenderer>() != null || model.transform.GetComponentsInChildren<MeshRenderer>() != null)
				{
					if (renderer != null)
						renderer.transform.gameObject.AddComponent<MeshCollider>();
				}
			}

		}
		else
		{
			Debug.LogError("Loading glTF failed!");
		}



		FindFirstObjectByType<ContentManager>().modelCount++;
		FindFirstObjectByType<ContentManager>().assets_Count++;

		if (FindFirstObjectByType<ContentManager>().assets_Count == FindFirstObjectByType<ContentManager>().totalNumber_of_assets)
		{
			Debug.Log("Downloaded All Assets : ");
			FindFirstObjectByType<ContentManager>().GetAnimationData();
			//GameObject.Find("TrackerParent").gameObject.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
		}
		if (FindFirstObjectByType<ContentManager>().modelCount < FindFirstObjectByType<ContentManager>().numberOfModels)
		{
			FindFirstObjectByType<ContentManager>().DownlodModel();
		}
	}

}
