using UnityEngine;
using System.Collections;

// Handles the placement of houses on the luthadel map to generate the city.
public class Environment_LuthadelMapGenerator : Environment {

    [SerializeField]
    private Texture2D map = null;

    // The sizes are in terms of pixels of the map texture, not meters.
    public GameObject[] houses = null;
    public int[] houseSizes = null;


    private Texture2D originalTexture;
    private int width, height;

    private void OnValidate() {
        originalTexture = map;
        height = map.height;
        width = map.width;
    }

    public void GenerateHouses() {

        /*
         * For each pixel:
         *  If it's black or blue, skip it. It's a road, river, etc.
         *  If it's white, it's valid zoning for a house. Place a house under certain conditions.
         *      Choose a house based on random chance and Luthadel zoning. If that house is surrounded by enough white pixels to fit, place it.
         *          Then, set those pixels to be blue, meaning the space is occupied.
         *          While we were checking nearby pixels to place the house, find the closest black pixel, and face the house towards it (make the house face a road)
         *      
         */

        float scaleX = transform.Find("Map").localScale.x;
        float scaleY = transform.Find("Map").localScale.y;
        
        Texture2D newTex = new Texture2D(width, height);
        newTex.filterMode = FilterMode.Point;
        Color[] pixels = map.GetPixels();
        for(int i = 0; i < height; i++) {
            for (int j = 0; j < width; j++) {
                // tests: place houses in a small section
                if(i >1300 && i < 1600 && j > 1300 && j < 1600) {

                    // if not white, skip
                    if(pixels[GetPixel(j, i)] != Color.white) {
                        continue;
                    }

                    // choose the building for this area
                    int houseIndex = 0;
                    int zoneSize = houseSizes[houseIndex];
                    bool bad = false;
                    // check if the zone has space (i.e. everything is white)
                    // Assumes the house's origin is at its XY center
                    // does not account for edge of screen out of bounds
                    int zj = 0, zi = 0;
                    for (zi = i - zoneSize/2; zi < i + zoneSize/2 && !bad; zi++) {
                        for (zj = j - zoneSize/2; zj < j + zoneSize/2; zj++) {
                            if (pixels[GetPixel(zj, zi)] != Color.white) {
                                bad = true;
                                break;
                            }
                        }
                    }
                    if (bad)
                        continue;
                    // Confirm zone by setting all to blue
                    for (zi = i - zoneSize / 2; zi < i + zoneSize / 2 && !bad; zi++) {
                        for (zj = j - zoneSize / 2; zj < j + zoneSize / 2; zj++) {
                            pixels[GetPixel(zj, zi)] = Color.blue;
                        }
                    }


                    // radial search around the zone to find the nearest road
                    // does not account for edge of screen
                    bool found = false;
                    int offset = 0;
                    while(offset < 10 && !found) {
                        zi = i - zoneSize / 2 - 1 - offset;
                        zj = j - zoneSize / 2 - offset;
                        while( zj < j + zoneSize/2+offset) {
                            if (pixels[GetPixel(zj, zi)] == Color.black) {
                                //pixels[GetPixel(zj, zi)] = Color.red;
                                found = true;
                                break;
                            } else {
                                //pixels[GetPixel(zj, zi)] = Color.green;
                                zj++;
                            }
                        }
                        while(zi < i + zoneSize / 2 + offset && !found) {
                            if (pixels[GetPixel(zj, zi)] == Color.black) {
                                //pixels[GetPixel(zj, zi)] = Color.red;
                                found = true;
                                break;
                            } else {
                                //pixels[GetPixel(zj, zi)] = Color.green;
                                zi++;
                            }
                        }
                        while (zj > j - zoneSize / 2-1- offset && !found) {
                            if (pixels[GetPixel(zj, zi)] == Color.black) {
                                //pixels[GetPixel(zj, zi)] = Color.red;
                                found = true;
                                break;
                            } else {
                                //pixels[GetPixel(zj, zi)] = Color.green;
                                zj--;
                            }
                        }
                        while (zi > i - zoneSize / 2-2- offset && !found) {
                            if (pixels[GetPixel(zj, zi)] == Color.black) {
                                //pixels[GetPixel(zj, zi)] = Color.red;
                                found = true;
                                break;
                            } else {
                                //pixels[GetPixel(zj, zi)] = Color.green;
                                zi--;
                            }
                        }
                        offset++;
                    }
                    Vector2 diff = new Vector2(zj - j, i - zi);
                    float rot_z = Mathf.Atan2(diff.y, diff.x) * Mathf.Rad2Deg;
                    Quaternion rotation = Quaternion.Euler(0, rot_z + 90, 0);

                    Vector3 worldPosition = new Vector3((j / (float)width - .5f) * scaleX, 0, (i / (float)height - .5f) * scaleY) + transform.position;
                    Instantiate(houses[houseIndex], worldPosition, rotation, transform);
                }
            }
        }
        newTex.SetPixels(pixels);
        newTex.Apply();

        transform.Find("Map").GetComponent<Renderer>().material.SetTexture("_MainTex", newTex);
    }
    private int GetPixel(int x, int y) {
        return y * width + x;
    }

    public void DestroyHouses() {
        for (int i = transform.childCount - 1; i >= 0; i--) {
            if(transform.GetChild(i).name.Contains("Luthadel_house")) {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
    public void ResetMapColors() {
        transform.Find("Map").GetComponent<Renderer>().material.SetTexture("_MainTex", originalTexture);
    }


}
