public class FreeNode : GeneralPlaceable
{
    public override bool[] Dof()
    {
        return new bool[] { true, true };
    }
}