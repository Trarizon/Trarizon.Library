using System.Numerics;
using Trarizon.Library.Game.Components.BehaviorTree;
using Trarizon.Library.Game.Testing.Core;

namespace Trarizon.Library.Game.Testing;
internal static class BehaviorTreeTests
{
    public static void BuildTree(Transform transform, Transform target)
    {
        var node = BehaviorTree.Selector(
            BehaviorTree.Sequence(
                new EnemyInAttackRangeCondition(transform, target),
                new AttackAction()),
            BehaviorTree.Sequence(
                new EnemyInViewRangeCondition(transform, target),
                new GoToAction()),
            new PatrolAction());

        dynamic builder = null!;

        builder
            .If(new EnemyInAttackRangeCondition(transform, target))
            .Then(new AttackAction())
            .ElseIf(new EnemyInViewRangeCondition(transform, target))
            .Then(new GoToAction())
            .Else(new PatrolAction());

    }

    struct Builder
    {
        public IfBuilder If(ConditionalBehaviorTreeNode condition) => new();

        public bool Build() => true;

        public struct IfBuilder
        {
            public ThenBuilder Then() => new();
        }

        public struct ThenBuilder
        {
            public IfBuilder ElseIf() => new();
            public Builder Else() => new();
        }
    }

    class EnemyInViewRangeCondition(Transform transform, Transform target) : ConditionalBehaviorTreeNode
    {
        protected override bool JudgeCondition()
        {
            var distance = Vector3.Distance(transform.Position, target.Position);
            return distance <= 10;
        }
    }

    class EnemyInAttackRangeCondition(Transform transform, Transform target) : ConditionalBehaviorTreeNode
    {
        protected override bool JudgeCondition()
        {
            var distance = Vector3.Distance(transform.Position, target.Position);
            return distance <= 5;
        }
    }

    class PatrolAction : ActionBehaviorTreeNode
    {
        private Transform _transform;
        private float _speed;
        private Transform[] _points;
        private int _nextPointIndex;

        protected internal override Status OnUpdate()
        {
            var target = _points[_nextPointIndex];
            var direction = target.Position - _transform.Position;
            var moveDistance = _speed * Time.DeltaTime;

            if (direction.Length() <= moveDistance) {
                _transform.Position = target.Position;
            }
            else {
                _transform.Position += Vector3.Normalize(direction) * moveDistance;
            }
            _nextPointIndex = (_nextPointIndex + 1) % _points.Length;

            _transform.Position = target.Position;
            return Status.Running;
        }
    }

    class GoToAction : ActionBehaviorTreeNode
    {
        private Transform _transform;
        private Transform _target;

        protected internal override Status OnUpdate()
        {
            return Status.Running;
        }
    }

    class AttackAction : ActionBehaviorTreeNode
    {

    }
}
