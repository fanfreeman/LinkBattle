using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

    public class StatusBarController : MonoBehaviour {

        public RectTransform fillerDecorater;
        public Image filler;
        public Text hpText;

        public int maxHealth = 100;
        public int currentHealth = 50;
        public float changeSpeed = 0.005f;

        //0-1
        private float currentPercentage;
        private float shoudBePercentage;

        private Vector3 initPosDecoraterVec3;


        void Start(){
            currentPercentage = (float)currentHealth/(float)maxHealth;
            shoudBePercentage = (float)currentHealth/(float)maxHealth;
            initPosDecoraterVec3 = fillerDecorater.position;
            //Debug.Log(currentHealth+"/"+maxHealth+"="+setPercentage);
            filler.fillAmount = currentPercentage;
            setHPText();
        }

        //初始化状态数值  血量。。
        public void setStatus(int setmaxHealth,int setcurrentHealth){
            maxHealth = setcurrentHealth;
            currentHealth = setcurrentHealth;

            currentPercentage = (float)currentHealth/(float)maxHealth;
            shoudBePercentage = (float)currentHealth/(float)maxHealth;
            initPosDecoraterVec3 = fillerDecorater.position;
            //Debug.Log(currentHealth+"/"+maxHealth+"="+setPercentage);
            filler.fillAmount = currentPercentage;
            setHPText();

        }
        // Use this for initialization
        void OnGUI () {
            setFiller();
            Vector3 posDecoraterVec3 = fillerDecorater.position;
            posDecoraterVec3.x =  initPosDecoraterVec3.x + filler.preferredWidth * filler.fillAmount;
            fillerDecorater.position = posDecoraterVec3;
        }

        private void setHPText(){
            hpText.text = currentHealth+"/"+maxHealth;
        }

        //正数加血负数减血
        public void changeHealth(int delta){
            currentHealth += delta;
            if(currentHealth > maxHealth){
                currentHealth = maxHealth;
            }else if (currentHealth < 0){
                currentHealth = 0;
            }
            setHPText();
            shoudBePercentage = (float)currentHealth/(float)maxHealth;
        }

        private void setFiller(){
            if(shoudBePercentage == filler.fillAmount)return;
            Debug.Log(shoudBePercentage);
            if(Mathf.Abs(shoudBePercentage - filler.fillAmount) > changeSpeed){
                if(shoudBePercentage > filler.fillAmount){
                    filler.fillAmount += changeSpeed;
                }else{
                    filler.fillAmount -= changeSpeed;
                }
            }else{
                filler.fillAmount = shoudBePercentage;
                currentPercentage = shoudBePercentage;
            }
        }



        // Update is called once per frame
        void Update () {

        }
    }
