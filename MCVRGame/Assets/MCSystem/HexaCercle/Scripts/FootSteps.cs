
using System.Collections.Generic;
using UnityEngine;

namespace MCData
{
    public class FootSteps
    {
        private int _moveFoot = 0;
        // 0 = not moving with foot, 1 = moving with left foot, 2 = moving with right foot
        //private float Ry, Ly, Gy, ToeLy, ToeRy;
        [SerializeField]
        //[FormerlySerializedAs("GROUNDOFFSET")]
        //private float GroundOffset = 0.02f;
        public Vector3[] startPosition = new Vector3[3];
        private Dictionary<GameObject, float> _initPosY;
        /// <summary>
        /// 启用/停用脚步移动功能
        /// </summary>
        private bool _canMove;


        /// <summary>
        /// 置1时，保持左脚着地不动，置2时为右脚
        /// </summary>
        public int MoveOnFoot
        {
            get
            {
                return _moveFoot;
            }
            set
            {
                if (value == 1 && _moveFoot != 1)
                    _footPosition = _footLeft.transform.position;
                else if (value == 2 && _moveFoot != 2)
                    _footPosition = _footRight.transform.position;
                _moveFoot = value;
            }
        }
        private GameObject _footLeft, _footRight, _toeLeft, _toeRight, _bodyRoot, _ground;
        private Vector3 _footPosition;

        public FootSteps()
        {
            _initPosY = new Dictionary<GameObject, float>();
            _ground = new GameObject("Ground");
            _ground.transform.SetPositionAndRotation(new Vector3(0, 0, 0), Quaternion.identity);
            _ground.transform.localScale = new Vector3(30, 1, 30);
        }

        public void SetFootObj(MotionCapture motion)
        {
            if (motion == null)
            {
                _canMove = false;
                return;
            }

            if (_footLeft == null)
                _footLeft = motion.foot_l;
            if (_footRight == null)
                _footRight = motion.foot_r;
            if (_toeLeft == null)
                _toeLeft = motion.footEnd_l;
            if (_toeRight == null)
                _toeRight = motion.footEnd_r;
            _bodyRoot = motion.RootObj;

            startPosition[0] = _bodyRoot.transform.position;
            startPosition[1] = _footLeft.transform.position;
            startPosition[2] = _footRight.transform.position;

            _initPosY.Clear();
            _initPosY.Add(_footLeft, _footLeft.transform.position.y);
            _initPosY.Add(_footRight, _footRight.transform.position.y);
            _initPosY.Add(_toeLeft, _toeLeft.transform.position.y);
            _initPosY.Add(_toeRight, _toeRight.transform.position.y);
        }



        public void Update()
        {
            if (!_canMove)
            {
                MoveOnFoot = 0;
                return;
            }
            CheckFootStep();
            if (MoveOnFoot == 1)
            {
                var delta = _footLeft.transform.position - _footPosition;
                delta.y -= AdjustModelY() / 2;
                _bodyRoot.transform.position -= delta;
            }
            else if (MoveOnFoot == 2)
            {
                var delta = _footRight.transform.position - _footPosition;
                delta.y -= AdjustModelY() / 2;
                _bodyRoot.transform.position -= delta;
            }
        }

        //通过脚的高度判断正在移动的脚
        private void CheckFootStep()
        {
            if (_footRight == null || _footLeft == null)
                return;

            if (_footRight.transform.position.y > _footLeft.transform.position.y)
            {
                MoveOnFoot = 1;
            }
            else
            {
                MoveOnFoot = 2;
            }
        }
        //防止双脚同时离开地面太高或者有一只下陷地面以下
        private float AdjustModelY()
        {
            var lowsetY = GetLowestY(out var initY);
            return initY - lowsetY;
        }

        private float GetLowestY(out float initY)
        {
            initY = 10;
            float lowestPosY = 10;
            foreach (var item in _initPosY)
            {
                var goPos = item.Key.transform.position;
                if (goPos.y < lowestPosY)
                {
                    lowestPosY = goPos.y;
                    initY = item.Value;
                }
            }
            return lowestPosY;
        }


        public void Reset()
        {
            SetEnable(false);
            _bodyRoot.transform.position = startPosition[0];
        }

        public void SetEnable(bool enable)
        {
            _canMove = enable;
        }

        public void Dispose()
        {
            _canMove = false;
            if (_ground != null)
                Object.Destroy(_ground);
        }
    }
}
