using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crate : MovingObject {

	public void Drag(int xDir, int yDir, RaycastHit2D hit)
	{
		Move (xDir, yDir, out hit);
	}

}
