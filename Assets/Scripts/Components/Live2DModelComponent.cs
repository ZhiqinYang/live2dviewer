using UnityEngine;
using System;
using live2d;
using live2d.framework;
using System.IO;

[ExecuteInEditMode]
public class Live2DModelComponent : MonoBehaviour {
    public TextAsset mocFile;
    public Texture2D[] textureFiles;

    private Live2DModelUnity live2DModel;
    private Matrix4x4 live2DCanvasPos;

	public Live2DModelUnity model {
		get { return live2DModel; }
	}

    void Start() {
        Live2D.init();
        Load();
    }

	bool Load() {
		if (mocFile != null) {
			Load(mocFile.bytes, textureFiles);
		}

		return live2DModel != null;
	}

	public void ReleaseModel() {
		if (live2DModel != null) {
			live2DModel.releaseModel();
			live2DModel = null;
		}
	}

	public void LoadFromFiles(string mocFile, string[] textureFiles) {
		var textures = new Texture2D[textureFiles.Length];
		for (int i = 0; i < textureFiles.Length; i++) {
			textures[i] = new Texture2D(2, 2);
			textures[i].LoadImage(File.ReadAllBytes(textureFiles[i]));
		}
		Load(File.ReadAllBytes(mocFile), textures);
	}

	public Live2DPartConfig[] LoadParts() {
		if (live2DModel != null) {
			var partsData = live2DModel.getModelImpl().getPartsDataList();
			var config = new Live2DPartConfig[partsData.Count];
			for (int i = 0; i < partsData.Count; i++) {
				var name = partsData[i].getPartsDataID().ToString();
				config[i] = new Live2DPartConfig {
					name = name,
					opacity = live2DModel.getPartsOpacity(name)
				};
			}

			return config;
		}

		return new Live2DPartConfig[0];
	}

	public void SetParts(Live2DPartConfig[] parts) {
		if (live2DModel == null) {
			return;
		}

		for (int i = 0; i < parts.Length; i++) {
			var p = parts[i];
			live2DModel.setPartsOpacity(i, p.opacity);
		}
	}

	void Load(byte[] moc, Texture2D[] textures) {
		if (live2DModel != null) {
			live2DModel.releaseModel();
		}

		live2DModel = Live2DModelUnity.loadModel(moc);

        for (int i = 0; i < textures.Length; i++) {
            live2DModel.setTexture(i, textures[i]);
        }

        float modelWidth = live2DModel.getCanvasWidth();
        live2DCanvasPos = Matrix4x4.Ortho(0, modelWidth, modelWidth, 0, -10.0f, 10.0f);
    }

    void Update() {
		if (live2DModel == null && !Load()) {
			return;
		}

        live2DModel.setMatrix(transform.localToWorldMatrix * live2DCanvasPos);

		if (!Application.isPlaying) {
			live2DModel.update();
			return;
		}

        live2DModel.update();
    }


    void OnRenderObject() {
		if (live2DModel == null && !Load()) {
			return;
		}

        if (live2DModel.getRenderMode() == Live2D.L2D_RENDER_DRAW_MESH_NOW) live2DModel.draw();
    }
}