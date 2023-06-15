using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Unity.Burst.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class Clicker : MonoBehaviour
{
    public GameObject Goat;
    public GameObject Tiger;
    public GameObject[] ValidPositions;
    public Transform Parent;
    public int goatCount;
    public bool goatTurn;
    public LayerMask LayerToHit;
    public GameObject[] Paths;
    public LayerMask PathLayer;
    public LayerMask AllGoats;
    public GameObject Tiger_Pospath;
    public bool goatTurnPre;

    // Start is called before the first frame update
    void Start()
    {
        goatTurn = true;
        goatTurnPre = goatTurn;
        var tigerPositions = GameObject.FindGameObjectsWithTag("Initial_TigerPos");
        foreach (var tigerPos in tigerPositions)
        {
            Instantiate(Tiger, tigerPos.transform.position, Quaternion.identity);
            Tiger.SetActive(true);
        }
        Debug.Log(Tiger.transform.position);
        //Goat.SetActive(false);       
        //Instantiate(Goat);
    }


    // Update is called once per frame
    void Update()
    {
        var maxDiags = GameObject.FindGameObjectsWithTag("MaxDiag");
        var maxDistance = Vector3.Distance(maxDiags[0].transform.position, maxDiags[1].transform.position) + 0.2;
        var mousePos = Input.mousePosition;
        var ray = Camera.main.ScreenToWorldPoint(mousePos);
        var hit = Physics2D.Raycast(ray, ray, 200, LayerToHit);
        var objSelected = GameObject.FindGameObjectWithTag("Move_Gotti");
               
        if (goatTurn)
        {
            if (Input.GetMouseButtonDown(0))
            {

                foreach (var ValidPosition in ValidPositions)
                {
                    Collider2D collider = ValidPosition.GetComponent<Collider2D>();
                    if (collider != null && collider.OverlapPoint(ray))
                    {
                        if (objSelected && hit.collider == null)
                        {
                            var diff = ValidPosition.transform.position - objSelected.transform.position;
                            var distance = Vector3.Distance(objSelected.transform.position, ValidPosition.transform.position);
                            var pathHits = Physics2D.RaycastAll(objSelected.transform.position, diff, distance, PathLayer);
                            if (pathHits.Length == 1)
                            {
                                var PathOrient = pathHits[0].collider.gameObject.transform;
                                //var PathOrientVector = PathOrient.GetComponent<Collider2D>().bounds.center - PathOrient.GetComponent<Collider2D>().bounds.min;
                                var Angles = pathHits[0].collider.gameObject.transform.eulerAngles;
                                var PathMagnitude = PathOrient.GetComponent<Collider2D>().bounds.extents.magnitude;
                                var PathOrientVector = new Vector3();
                                PathOrientVector.x = PathMagnitude * (float)Math.Cos((Math.PI / 180) * Angles.z);
                                PathOrientVector.y = PathMagnitude * (float)Math.Sin((Math.PI / 180) * Angles.z);
                                PathOrientVector.z = PathMagnitude * (float)Math.Sin((Math.PI / 180) * Angles.y);
                                var dotProduct = Vector3.Dot(PathOrientVector, diff);
                                if (dotProduct == 0)
                                {
                                    objSelected.transform.position = ValidPosition.transform.position;
                                    objSelected.GetComponentInChildren<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
                                    objSelected.tag = "Untagged";
                                    //Debug.Log("moved");
                                    //Debug.Log(PathOrientBounds);
                                    //Debug.Log(diff);
                                    //Debug.Log(PathOrientMagnitude);
                                    //Debug.Log(PathOrientBounds.magnitude);
                                    goatTurn = !goatTurn;
                                }
                                else
                                {
                                    Debug.Log("It isn't perpendicular");
                                    Debug.Log(dotProduct);
                                    
                                }
                            }
                            break;
                        }
                        else if (objSelected != null && hit.collider != null)
                        {
                            objSelected.GetComponentInChildren<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
                            objSelected.tag = "Untagged";
                            Debug.Log("UnSelect");

                        }
                        else if (hit.collider == null && goatCount < 20)
                        {
                            //Debug.Log(hit.collider.gameObject.name);
                            // hit.collider.gameObject.GetComponent<SpriteRenderer>().color = new Color(174, 25, 0, 1);

                            //Goat.transform.position = ValidPosition.transform.position;
                            Instantiate(Goat, ValidPosition.transform.position, Quaternion.identity);
                            Goat.SetActive(true);
                            Debug.Log("Specified Position Reached");
                            goatCount++;
                            Debug.Log(goatCount);
                            goatTurn = !goatTurn;
                            break;
                        }
                        else if (hit.collider != null && hit.collider.gameObject.name == "Goat(Clone)" && objSelected == null && goatCount >= 20)
                        {
                            hit.collider.gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(174f / 255f, 25f / 255f, 0f, 1f);
                            hit.collider.gameObject.tag = "Move_Gotti";
                            break;
                        }
                    }

                }
                               
            }
        }
        else
        {
            if (Input.GetMouseButtonDown(0))
            {
                // var dirRay = Camera.main.ScreenPointToRay(mousePos);
                //var objSelected = GameObject.FindGameObjectWithTag("Move_Gotti");
                foreach (var ValidPosition in ValidPositions)
                {
                    Collider2D collider = ValidPosition.GetComponent<Collider2D>();
                    if (collider != null && collider.OverlapPoint(ray))
                    {
                        if (objSelected && hit.collider == null)
                        {
                            var diff = ValidPosition.transform.position - objSelected.transform.position;
                            var distance = Vector3.Distance(objSelected.transform.position, ValidPosition.transform.position);
                            var pathHits = Physics2D.RaycastAll(objSelected.transform.position, diff, distance, PathLayer);
                            var killerHits = Physics2D.RaycastAll(objSelected.transform.position, diff, distance, AllGoats);
                            if (killerHits.Length == 2 && pathHits.Length == 2)
                            {
                                int i;
                                var ogVectors = new List<Vector3>();
                                var PathOrientBounds = new List<Vector3>();
                                var dotProducts = new List<float>();
                                for (i=0; i < 2; i++)
                                {
                                    //PathOrientBounds.Add(pathHits[i].collider.gameObject.transform.GetComponent<Collider2D>().bounds.max - pathHits[i].collider.gameObject.transform.GetComponent<Collider2D>().bounds.min);
                                    //Debug.Log(PathOrientBounds[i]);
                                    var PathOrient = pathHits[i].collider.gameObject.transform;
                                    //var PathOrientVector = PathOrient.GetComponent<Collider2D>().bounds.center - PathOrient.GetComponent<Collider2D>().bounds.min;
                                    var Angles = pathHits[i].collider.gameObject.transform.eulerAngles;
                                    var PathMagnitude = PathOrient.GetComponent<Collider2D>().bounds.extents.magnitude;
                                    var PathOrientVector = new Vector3();
                                    PathOrientVector.x = PathMagnitude * (float)Math.Cos((Math.PI/180)*Angles.z);
                                    PathOrientVector.y = PathMagnitude * (float)Math.Sin((Math.PI/180) * Angles.z);
                                    PathOrientVector.z = PathMagnitude * (float)Math.Sin((Math.PI/180) * Angles.y);
                                    PathOrientBounds.Add(PathOrientVector);
                                    //Debug.Log(PathOrientBounds[i]);
                                    //Debug.Log("The angles are " + Angles);
                                    //Debug.Log(diff);
                                    dotProducts.Add(Vector3.Dot(PathOrientBounds[i], diff));
                                    //Debug.Log(dotProducts[i]);
                                }
                                if (dotProducts.All(x => Math.Abs(x) < 0.001) == true)
                                {
                                    Debug.Log("Killer");
                                    Debug.Log(pathHits.Length);
                                    Destroy(killerHits[1].collider.gameObject);
                                    objSelected.transform.position = ValidPosition.transform.position;
                                    objSelected.GetComponentInChildren<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
                                    objSelected.tag = "Tiger";
                                    goatTurn = !goatTurn;
                                    Debug.Log("Killed the goat");
                                }
                                else
                                {
                                    Debug.Log("Seems like it didn't work out");
                                }
                            }
                            //    if (pathHits.Length == 1)
                            //    {
                            //        objSelected.transform.position = ValidPosition.transform.position;
                            //        objSelected.GetComponentInChildren<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
                            //        objSelected.tag = "Untagged";
                            //        goatTurn = !goatTurn;
                            //        Debug.Log("path is working");
                            //    }
                            //    break;
                            //}
                            if (pathHits.Length == 1)
                            {
                                var PathOrient = pathHits[0].collider.gameObject.transform;
                                //var PathOrientVector = PathOrient.GetComponent<Collider2D>().bounds.center - PathOrient.GetComponent<Collider2D>().bounds.min;
                                var Angles = pathHits[0].collider.gameObject.transform.eulerAngles;
                                var PathMagnitude = PathOrient.GetComponent<Collider2D>().bounds.extents.magnitude;
                                var PathOrientVector = new Vector3();
                                PathOrientVector.x = PathMagnitude * (float)Math.Cos((Math.PI / 180) * Angles.z);
                                PathOrientVector.y = PathMagnitude * (float)Math.Sin((Math.PI / 180) * Angles.z);
                                PathOrientVector.z = PathMagnitude * (float)Math.Sin((Math.PI / 180) * Angles.y);
                                var dotProduct = Vector3.Dot(PathOrientVector, diff);
                                if (Math.Abs(dotProduct) < 0.001)
                                {
                                    objSelected.transform.position = ValidPosition.transform.position;
                                    objSelected.GetComponentInChildren<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
                                    objSelected.tag = "Tiger";
                                    //Debug.Log("moved");
                                    //Debug.Log(PathOrientBounds);
                                    //Debug.Log(diff);
                                    //Debug.Log(PathOrientMagnitude);
                                    //Debug.Log(PathOrientBounds.magnitude);
                                    goatTurn = !goatTurn;
                                }
                                else
                                {
                                    Debug.Log("It isn't perpendicular");
                                }
                            }
                                break;
                            }
                            else if (objSelected != null && hit.collider != null)
                            {
                                objSelected.GetComponentInChildren<SpriteRenderer>().color = new Color(255f / 255f, 255f / 255f, 255f / 255f, 1f);
                                objSelected.tag = "Tiger";
                                Debug.Log("UnSelect");

                            }
                            else if (hit.collider != null && hit.collider.gameObject.name == "Tiger(Clone)" && objSelected == null)
                            {
                                //Debug.Log("habibi");
                                hit.collider.gameObject.GetComponentInChildren<SpriteRenderer>().color = new Color(174f / 255f, 25f / 255f, 0f, 1f);
                                hit.collider.gameObject.tag = "Move_Gotti";
                                Debug.Log("OI");
                                break;
                            }
                        }
                    }

                }
            }
        if (goatTurnPre != goatTurn)
        {
            var tigers = GameObject.FindGameObjectsWithTag("Tiger");
            foreach (var tiger in tigers)
            {
                //Debug.Log(tiger.transform.position);
                Instantiate(Tiger_Pospath, tiger.transform.position, Quaternion.identity);
                Tiger_Pospath.SetActive(true);
                //colliders = Physics2D.OverlapBox(Tiger_Pospath.GetComponent<BoxCollider2D>().bounds).Where(collider => collider.gameobjrct.tag = "path");.Where(x => x.gameObject.layer == 7);
                //Collider2D[] objectsCollides = new Collider2D[];
                List<Collider2D> objectsCollides = new List<Collider2D>();
                Tiger_Pospath.GetComponent<Collider2D>().OverlapCollider(new ContactFilter2D()
                {
                    useLayerMask = true,
                    layerMask = (1 << 7)
                },objectsCollides);
                foreach(var collider in objectsCollides)
                {
                    Debug.Log(collider.gameObject.name);
                }
                Debug.Log(objectsCollides.Count());
                Debug.Log(tiger.transform.position);
            }
            Debug.Log("Another run");
            goatTurnPre = goatTurn;
        }
        }

        }
        
    

