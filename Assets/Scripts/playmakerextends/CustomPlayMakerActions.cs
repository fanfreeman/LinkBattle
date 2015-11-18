using UnityEngine;

namespace HutongGames.PlayMaker.Actions{
	[ActionCategory("特殊技")]
	[Tooltip("选择一个随机敌人，这个敌人不是城墙，不是蓄力单位，未失去控制")]
	public class SelectRandomUnitFromOtherSide : FsmStateAction
    {
        [RequiredField]
        [Tooltip("Caster Unit")]
		public Unit unit;

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
            theRandomUnit.Value = BoardManager.instance.GetControllableUnitFromOtherByRandomArray(isAtBottom);
        }

        public override void Reset()
        {
            theRandomUnit = null;
        }

        public override string ErrorCheck()
        {
			if(unit == null){
                return "unit should not be null";
            }
            return string.Empty;
        }
    }

    [ActionCategory("特殊技")]
    [Tooltip("让某个单位param1失去控制！这个单位不是城墙，不是蓄力单位，未失去控制！")]
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
            turnOfLossControl = 1;//默认一轮
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

}

