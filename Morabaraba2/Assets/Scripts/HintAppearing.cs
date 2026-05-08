using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HintAppearing : MonoBehaviour
{
    public GameObject hintBtnOne;
    public GameObject hintBtnTwo;
    public GameObject hintBtnThree;

    public float hintOneDelay = 10f;
    public float hintTwoDelay = 20f;
    public float hintThreeDelay = 30f;

    public float allHintsOff = 40f;
    // Start is called before the first frame update
    void Start()
    {
        hintBtnOne.SetActive(false);
        hintBtnTwo.SetActive(false);
        hintBtnThree.SetActive(false);
        
        StartCoroutine(ActivateHints());
        
    }

    private IEnumerator ActivateHints()
    {
        yield return new WaitForSeconds(hintOneDelay);
        hintBtnOne.SetActive(true);
        
        
        yield return new WaitForSeconds(hintTwoDelay-hintOneDelay);
        hintBtnOne.SetActive(false);
        hintBtnTwo.SetActive(true);
            
        
        
        yield return new WaitForSeconds(hintThreeDelay-hintTwoDelay);
        hintBtnTwo.SetActive(false);
        hintBtnThree.SetActive(true);

        yield return new WaitForSeconds(allHintsOff - hintThreeDelay);
        hintBtnThree.SetActive(false);



    }
}
