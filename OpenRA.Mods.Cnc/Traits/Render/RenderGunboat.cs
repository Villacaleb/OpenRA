#region Copyright & License Information
/*
 * Copyright 2007-2015 The OpenRA Developers (see AUTHORS)
 * This file is part of OpenRA, which is free software. It is made
 * available to you under the terms of the GNU General Public License
 * as published by the Free Software Foundation. For more information,
 * see COPYING.
 */
#endregion

using System.Linq;
using OpenRA.Graphics;
using OpenRA.Mods.Common.Traits;
using OpenRA.Traits;

namespace OpenRA.Mods.Cnc.Traits
{
	class RenderGunboatInfo : RenderSpritesInfo, IQuantizeBodyOrientationInfo, Requires<IBodyOrientationInfo>
	{
		[Desc("Turreted 'Turret' key to display")]
		public readonly string Turret = "primary";

		public readonly string LeftSequence = "left";
		public readonly string RightSequence = "right";
		public readonly string WakeLeftSequence = "wake-left";
		public readonly string WakeRightSequence = "wake-right";

		public override object Create(ActorInitializer init) { return new RenderGunboat(init, this); }

		public int QuantizedBodyFacings(ActorInfo ai, SequenceProvider sequenceProvider, string race)
		{
			return 2;
		}
	}

	class RenderGunboat : RenderSprites, INotifyDamageStateChanged
	{
		Animation left, right;

		public RenderGunboat(ActorInitializer init, RenderGunboatInfo info)
			: base(init, info)
		{
			var name = GetImage(init.Self);
			var facing = init.Self.Trait<IFacing>();
			var turret = init.Self.TraitsImplementing<Turreted>()
				.First(t => t.Name == info.Turret);

			left = new Animation(init.World, name, () => turret.TurretFacing);
			left.Play(info.LeftSequence);
			Add(info.LeftSequence, new AnimationWithOffset(left, null, () => facing.Facing > 128, 0));

			right = new Animation(init.World, name, () => turret.TurretFacing);
			right.Play(info.RightSequence);
			Add(info.RightSequence, new AnimationWithOffset(right, null, () => facing.Facing <= 128, 0));

			var leftWake = new Animation(init.World, name);
			leftWake.PlayRepeating(info.WakeLeftSequence);
			Add(info.WakeLeftSequence, new AnimationWithOffset(leftWake, null, () => facing.Facing > 128, -87));

			var rightWake = new Animation(init.World, name);
			rightWake.PlayRepeating(info.WakeRightSequence);
			Add(info.WakeRightSequence, new AnimationWithOffset(rightWake, null, () => facing.Facing <= 128, -87));
		}

		public void DamageStateChanged(Actor self, AttackInfo e)
		{
			left.ReplaceAnim(NormalizeSequence(left, e.DamageState, left.CurrentSequence.Name));
			right.ReplaceAnim(NormalizeSequence(right, e.DamageState, right.CurrentSequence.Name));
		}
	}
}
