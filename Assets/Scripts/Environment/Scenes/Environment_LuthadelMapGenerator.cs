using UnityEngine;
using System.Collections;

public class Environment_LuthadelMapGenerator : Environment {

    [SerializeField]
    private GameObject house;
    [SerializeField]
    private Texture2D map;
    [SerializeField]
    private float mapWidth, mapHeight;

    public void PlaceHouse() {

        Color[] pixels = map.GetPixels();
        int height = map.height;
        int width = map.width;
        for(int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                if(i == j) {
                    Vector2 position = new Vector2(j / width * mapWidth, i / height * mapHeight);
                    Instantiate(house, position, Quaternion.identity, transform);
                }
            }
        }

    }
}
