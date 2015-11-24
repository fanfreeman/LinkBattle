using HutongGames.PlayMaker;
using System.Collections.Generic;
using UnityEngine;

namespace HutongGames.PlayMaker.Actions{
	[ActionCategory("特殊技")]
	[Tooltip("选择一个随机敌人，这个敌人不是城墙，不是蓄力单位，未失去控制")]
	public class SelectRandomUnitFromOtherSide : FsmStateAction
    {
        [RequiredField]
        [Tooltip("Caster Unit")]
		public Unit unit;

        [RequiredField]
        [Tooltip("能被控制也没事干的单位")]
        public bool idleUnit = true;//随机选择的敌人存储到这里 可以为null

        [RequiredField]
        [Tooltip("城墙要不要")]
        public bool barricadUnit;//随机选择的敌人存储到这里 可以为null

        [RequiredField]
        [Tooltip("三联单位要不要")]
        public bool chargingUnit;//随机选择的敌人存储到这里 可以为null

        [RequiredField]
        [Tooltip("失去控制的单位要不要")]
        public bool lossControllUnit;//随机选择的敌人存储到这里 可以为null

        [RequiredField]
        [Tooltip("虚无的单位要不要")]
        public bool nihility;//随机选择的敌人存储到这里 可以为null


        [UIHint(UIHint.Variable)]
        [RequiredField]
        [Tooltip("Store the seleted unit!")]
        public FsmObject theRandomUnit;//随机选择的敌人存储到这里 可以为null

        /// <summary>
        ///随机获取一个敌人可以控制的随机单位 为FSM调用而制作
        /// </summary>
        public override void OnEnter()
        {
            bool isAtBottom = unit.isAtBottom;
            theRandomUnit.Value =
            BoardManager.instance.GetControllableUnitFromOtherByRandomArray(
                    isAtBottom,chargingUnit,lossControllUnit,barricadUnit,nihility,idleUnit
            );
            if(theRandomUnit.Value == null || theRandomUnit == null)
            Debug.Log("Random is Null!!!!!!!!!!!!" );
        }

        public override void Reset()
        {
            theRandomUnit = null;
            chargingUnit = false;
            lossControllUnit = false;
            idleUnit = true;
            barricadUnit = false;
            nihility = false;

        }

        public override string ErrorCheck()
        {
			if(unit == null)
            {
                return "unit should not be null";
            }

            if(
            chargingUnit == false&&
            lossControllUnit == false&&
            barricadUnit == false&&
            nihility == false&&
            idleUnit == false)
            {
                return "我看你啥都不用选了！";
            }
            return string.Empty;
        }
    }

    [ActionCategory("特殊技")]
    [Tooltip("让某个单位失去控制！这个单位不是城墙，不是蓄力单位，未失去控制！")]
    public class ForceEnemyLossControll : FsmStateAction{
        [RequiredField]
        [Tooltip("要失去控制的单位")]
        [UIHint(UIHint.Variable)]
        public FsmObject lossControllUnitRef;

        [RequiredField]
        [Tooltip("失控时的效果。例如:被变成石头")]
        public FsmGameObject lossControlPerfab;

        [RequiredField]
        [Tooltip("失控轮数")]
        public FsmInt turnOfLossControl;

        //失去控制状态
        public static void SetToLossControlStatus(Unit lossControllUnit, GameObject lossControlPerfab, int lossControllTurn = 2)
        {
            lossControllUnit.isLossControll = true;
        }

        public override void OnEnter()
        {
            Unit lossControllUnit = lossControllUnitRef.Value as Unit;
            if (lossControllUnit != null)
            lossControllUnit.LossControll(lossControlPerfab.Value, turnOfLossControl.Value);
        }

        public override void Reset()
        {
            lossControllUnitRef = null;
            turnOfLossControl = 2;
        }

        public override string ErrorCheck()
        {
            if (lossControllUnitRef == null)
            {
                return "Loss Controll Unit must not be null";
            }

            if (lossControllUnitRef.GetType() != typeof(FsmObject))
            {
                return "Loss Controll wrong type";
            }
            return string.Empty;
        }
    }

    [ActionCategory("特殊技")]
    [Tooltip("让某个单位进入虚无状态！")]
    public class ForceEnemyNihility : FsmStateAction{
        [RequiredField]
        [Tooltip("要失去控制的单位")]
        [UIHint(UIHint.Variable)]
        public FsmObject nihilityUnitRef;

        [RequiredField]
        [Tooltip("失控时的效果。例如:被变成石头")]
        public FsmGameObject nihilityPerfab;

        [RequiredField]
        [Tooltip("失控轮数")]
        public FsmInt turnOfNihility;

        //失去控制状态
        public override void OnEnter() {
//            Debug.Log("TypeName:"+nihilityUnitRef.TypeName);
//            Debug.Log("typeof:"+typeof(Unit).ToString());
            if (nihilityUnitRef.TypeName ==  typeof(Unit).ToString())
            {
                Unit nihilityUnit = nihilityUnitRef.Value as Unit;
                if (nihilityUnit != null)
                    nihilityUnit.Nihility(nihilityPerfab.Value, turnOfNihility.Value);
            }
            else
            {
                PlaymakerUnitArray units = nihilityUnitRef.Value as PlaymakerUnitArray;
                List<Unit> result = units.GetUnits();
                foreach (Unit unit in result)
                {
                    Debug.Log("result is null??:" + unit == null);
                }
                foreach (Unit unit in result)
                {
                    unit.Nihility(nihilityPerfab.Value, turnOfNihility.Value);
                }
            }
        }

        public override void Reset()
        {
            nihilityUnitRef = null;
        }

        public override string ErrorCheck()
        {
            if (nihilityUnitRef == null)
            {
                return "Loss Controll Unit must not be null";
            }

            if (nihilityUnitRef.GetType() != typeof(FsmObject))
            {
                return "Loss Controll wrong type";
            }
            return string.Empty;
        }
    }

    [ActionCategory("特殊技")]
    [Tooltip("获得所有的敌人")]
    public class SelcetAllOtherSideUnit : FsmStateAction
    {
        [UIHint(UIHint.Variable)]
        [RequiredField]
        [Tooltip("自己是上班还是下边")]
        public FsmBool belongsToBottomPlayer;//随机选择的敌人存储到这里 可以为null

        [UIHint(UIHint.Variable)]
        [RequiredField]
        [Tooltip("Must be type PlaymakerUnitArray")]
        public FsmObject unitUnion;//随机选择的敌人存储到这里 可以为null

        /// <summary>
        ///选取所有敌人
        /// </summary>
        public override void OnEnter()
        {
            PlaymakerUnitArray output = unitUnion.Value as PlaymakerUnitArray;
            List<Unit> temp = new List<Unit>();
            if (belongsToBottomPlayer.Value)
            {
                //三联只要队长
                foreach (Unit unit in BoardManager.instance.unitGridTop)
                {
                    if(unit !=null)
                        if(!(unit.isActivated && !unit.isChargeUpFlagHolder))
                        {
                            temp.Add(unit);
                        }
                }
                output.SetUnits(temp);
            }
            else
            {
                foreach (Unit unit in BoardManager.instance.unitGridBottom)
                {
                    if(unit !=null)
                    if(!(unit.isActivated && !unit.isChargeUpFlagHolder))
                    {
                        temp.Add(unit);
                    }
                }
                output.SetUnits(temp);
            }
        }


        public override void Reset()
        {
            belongsToBottomPlayer = null;
            unitUnion = null;
        }

        public override string ErrorCheck()
        {
            if(unitUnion.TypeName != typeof(PlaymakerUnitArray).ToString())
                return "error return parameter";
            if(belongsToBottomPlayer == null)
                return "your side!";

            return string.Empty;
        }
    }


}

