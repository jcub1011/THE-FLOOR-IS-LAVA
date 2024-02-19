using Godot;
using System;

namespace Players;

public partial class HorizontalMovementHandler : Node
{
    [Export] PlayerController _body;
    [Export] float _moveSpeed;
}
