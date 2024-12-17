using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

public class StructureSolverAndAnalyzer
{
    public TrussStructure2D structure { get; private set; }
    public StructureSolverAndAnalyzer(ref List<Truss> trussesVR,ref List<Force> forcesVR,ref List<GeneralPlaceable> nodesVR)
    {
        DeleteNodes(GetIsolatedNodes(nodesVR, trussesVR), ref nodesVR);
        PopulateStructure(trussesVR, forcesVR, nodesVR);
    }
    
    public bool Solve()
    {
        bool result = structure.Solve();
        return result;
    }
    public void PopulateStructure(List<Truss> trussesVR, List<Force> forcesVR, List<GeneralPlaceable> nodesVR)
    {
        List<Node2D> nodes = new List<Node2D>();
        List<TrussElement2D> trusses = new List<TrussElement2D>();
        List<Force2D> forces = new List<Force2D>();

        // Convert AltLehelyezheto objects to Node2D objects
        foreach (GeneralPlaceable nodeVR in nodesVR)
        {
            Node2D node = new Node2D(nodeVR.transform.position.x, nodeVR.transform.position.y);
            if (!nodeVR.Dof()[0] && !nodeVR.Dof()[1])
            {
                node.boundaryCondition = BoundaryCondition.Pinned;
            }
            else if (!nodeVR.Dof()[1])
            {
                node.boundaryCondition = BoundaryCondition.xRoller;
            }
            else if (!nodeVR.Dof()[0])
            {
                node.boundaryCondition = BoundaryCondition.yRoller;
            }
            else
            {
                node.boundaryCondition = BoundaryCondition.Free;
            }
            nodes.Add(node);
        }
        // Convert Truss objects to TrussElement2D objects
        foreach (Truss trussVR in trussesVR)
        {
            Node2D node1 = nodes[nodesVR.IndexOf(trussVR.neighbours[0].GetComponent<GeneralPlaceable>())];
            Node2D node2 = nodes[nodesVR.IndexOf(trussVR.neighbours[1].GetComponent<GeneralPlaceable>())];
            trusses.Add(new TrussElement2D(node1, node2));
        }
        // Convert Force objects to Force2D objects
        foreach (Force forceVR in forcesVR)
        {
            Node2D node = nodes[nodesVR.IndexOf(forceVR.neighbours[0].GetComponent<GeneralPlaceable>())];
            Vector3 direction = forceVR.transform.rotation * Vector3.forward;
            forces.Add(new Force2D(node, forceVR.size * direction.x*10000, forceVR.size * direction.y*10000, ForceType.Outer));
        }

        structure = new TrussStructure2D(nodes, trusses, forces);
    }
    public List<GeneralPlaceable> GetIsolatedNodes(List<GeneralPlaceable> nodesVR, List<Truss> trussesVR)
    {
        List<GeneralPlaceable> notisolatedNodes = new List<GeneralPlaceable>();
        List<GeneralPlaceable> isolatedNodes = new List<GeneralPlaceable>();

        foreach (Truss element in trussesVR)
        {
            foreach (GameObject szomszed in element.neighbours)
            {
                if (!notisolatedNodes.Contains(szomszed.GetComponent<GeneralPlaceable>()))
                    notisolatedNodes.Add(szomszed.GetComponent<GeneralPlaceable>());
            }
        }

        foreach (GeneralPlaceable altLehelyezheto in nodesVR)
        {
            if (!notisolatedNodes.Contains(altLehelyezheto))
                isolatedNodes.Add(altLehelyezheto);
        }
        return isolatedNodes;
    }
    public void DeleteNodes(List<GeneralPlaceable> deleteables, ref List<GeneralPlaceable> nodesVR)
    {
        foreach (GeneralPlaceable node in deleteables)
        {
            if (nodesVR.Contains(node))
            {
                nodesVR.Remove(node);
                node.DeleteObject();
            }
        }
    }

    //some more experimental checks for the structure
    /*public List<Truss> GetFixedTrusses(List<>)
    {
        List<TrussElement2D> fixedTrusses = new List<TrussElement2D>();

        foreach (TrussElement2D element in structure.Elements)
        {
            if (IsNodeFixed(element.Node1) && IsNodeFixed(element.Node2))
            {
                fixedTrusses.Add(element);
            }
        }

        return fixedTrusses;
    }

    public bool IsNodeFixed(Node2D node)
    {
        foreach (Support2D support in structure.Supports)
        {
            if (support.Node == node && support.XFixed && support.YFixed)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsStructureConnected(List<GeneralPlaceable> nodesVR, List<Truss> trussesVR)
    {
        List<GeneralPlaceable> visitedNodes = new List<GeneralPlaceable>();
        TraverseStructure(nodesVR[0], ref visitedNodes, trussesVR);

        return visitedNodes.Count == nodesVR.Count;
    }

    private void TraverseStructure(GeneralPlaceable node, ref List<GeneralPlaceable> visitedNodes, List<Truss> trussesVR)
    {
        visitedNodes.Add(node);

        foreach (Truss element in trussesVR)
        {

            if (element.neighbours[0].GetComponent<GeneralPlaceable>() == node && !visitedNodes.Contains(element.neighbours[1].GetComponent<GeneralPlaceable>()))
            {
                TraverseStructure(element.neighbours[1].GetComponent<GeneralPlaceable>(), ref visitedNodes, trussesVR);
            }
            else if (element.neighbours[1].GetComponent<GeneralPlaceable>() == node && !visitedNodes.Contains(element.neighbours[0].GetComponent<GeneralPlaceable>()))
            {
                TraverseStructure(element.neighbours[0].GetComponent<GeneralPlaceable>(), ref visitedNodes, trussesVR);
            }
        }
    }

    public bool IsGridStaticallyDeterminate(List<Truss> trussesVR, List<AltLehelyezheto> nodesVR)
    {
        StructureSolverAndAnylyzer analyzer = new StructureSolverAndAnylyzer(trussesVR, null, nodesVR);
        int numNodes = nodesVR.Count;
        int numEquations = 2 * numNodes; // Each node has two unknown reactions (X and Y)
        int numUnknownReactions = 0;

        foreach (AltLehelyezheto node in nodesVR)
        {
            if (!analyzer.IsNodeFixed(node))
            {
                numUnknownReactions += 2; // X and Y reactions are unknown for non-fixed nodes
            }
        }

        return numUnknownReactions == numEquations;
    }*/
}
public class TrussStructure2D
{
    public List<Node2D> nodes { get; private set; }
    public List<TrussElement2D> trusses { get; private set; }
    public List<Force2D> forces { get; private set; }
    //public List<Support2D> supports { get; private set; }

    public double[] globalDisplacementVector { get; private set; }
    public double[] globalForcesVector { get; private set; }

    public TrussStructure2D(List<Node2D> nodes, List<TrussElement2D> elements, List<Force2D> forces/*, List<Support2D> supports*/)
    {
        this.nodes = nodes;
        this.trusses = elements;
        this.forces = forces;
        //this.supports = supports;
    }
    public bool Solve()
    {
        double[,] globalStiffnessMatrix = MergeStiffnessMatrices();
        double[,] globalCondensedStiffnessMatrix = CondenseStiffnessMatrix(globalStiffnessMatrix);
        double[] globalCondensedForceVector = CondenseForceVector(MergeForceVectors());
        double[] globalCondensedDisplacementVector = SolveLinearSystem(globalCondensedStiffnessMatrix, globalCondensedForceVector);
        foreach(double d in globalCondensedDisplacementVector)
        {
            if (double.IsNaN(d))
            {
                return false;
            }
        }
        globalDisplacementVector = GetFullDisplacementVector(globalCondensedDisplacementVector);
        globalForcesVector = MultiplyMatrixVector(globalStiffnessMatrix, globalDisplacementVector);
        FillValues();
        return true;
    }
    public static double[] FwdSubstitution(double[,] L, double[] b)
    {
        int n = b.Length;
        double[] y = new double[n];

        for (int i = 0; i < n; i++)
        {
            double sum = 0;

            for (int j = 0; j < i; j++)
            {
                sum += y[j] * L[i, j];
            }

            y[i] = (b[i] - sum) / L[i, i];
        }

        return y;
    }
    public static double[] BwdSubstitution(double[,] U, double[] y)
    {
        int n = y.Length;
        double[] x = new double[n];

        for (int i = n - 1; i >= 0; i--)
        {
            double sum = 0;

            for (int j = i + 1; j < n; j++)
            {
                sum += U[i, j] * x[j];
            }

            x[i] = (y[i] - sum) / U[i, i];
        }

        return x;
    }
    public void LUdcmpMethod(double[,] A, ref double[,] Lref, ref double[,] Uref)
    {
        int N = A.GetLength(0);

        // If N = 1, we are done
        if (N == 1)
        {
            double[,] L2 = new double[1, 1];
            L2[0, 0] = 1;
            double[,] U2 = A;
            Lref = L2;
            Uref = U2;
            return;
        }

        // Write A as a 2x2 matrix
        double a11 = A[0, 0];
        double[,] a12 = new double[N - 1, N - 1];
        for (int i = 0; i < N - 1; i++)
        {
            for (int j = 0; j < N - 1; j++)
            {
                a12[i, j] = A[0, j + 1];
            }
        }
        double[,] a21 = new double[N - 1, N - 1];
        for (int i = 0; i < N - 1; i++)
        {
            for (int j = 0; j < N - 1; j++)
            {
                a21[i, j] = A[i + 1, 0];
            }
        }
        double[,] A22 = new double[N - 1, N - 1];
        for (int i = 0; i < N - 1; i++)
        {
            for (int j = 0; j < N - 1; j++)
            {
                A22[i, j] = A[i + 1, j + 1];
            }
        }

        // Compute first row and column of L, U matrices
        double l11 = 1;
        double[,] l12 = new double[N - 1, N - 1];
        double u11 = a11;
        double[,] u12 = a12;
        double[,] l21 = new double[N - 1, N - 1];
        for (int i = 0; i < N - 1; i++)
        {
            l21[i, 0] = (1 / u11) * a21[i, 0];
        }

        double[,] S22 = new double[N - 1, N - 1];
        for (int i = 0; i < N - 1; i++)
        {
            for (int j = 0; j < N - 1; j++)
            {
                S22[i, j] = A22[i, j] - l21[i, 0] * u12[0, j];
            }
        }

        // Recursively solve subproblem of small size
        double[,] L22 = null;
        double[,] U22 = null;
        LUdcmpMethod(S22, ref L22, ref U22);

        // Put the L, U matrices together
        double[,] L = new double[N, N];
        L[0, 0] = l11;
        for (int i = 0; i < N - 1; i++)
        {
            L[0, i + 1] = l12[i, 0];
        }
        for (int i = 0; i < N - 1; i++)
        {
            L[i + 1, 0] = l21[i, 0];
            for (int j = 0; j < N - 1; j++)
            {
                L[i + 1, j + 1] = L22[i, j];
            }
        }

        double[,] U = new double[N, N];
        U[0, 0] = u11;
        for (int i = 0; i < N - 1; i++)
        {
            U[0, i + 1] = u12[0, i];
        }
        for (int i = 0; i < N - 1; i++)
        {
            U[i + 1, 0] = 0;
            for (int j = 0; j < N - 1; j++)
            {
                U[i + 1, j + 1] = U22[i, j];
            }
        }
        Lref = L;
        Uref = U;
        return;
    }
    public void FillValues()
    {
        // Fill the displacement values of the nodes
        for (int i = 0; i < nodes.Count; i++)
        {
            nodes[i].xDisplacement = globalDisplacementVector[2 * i];
            nodes[i].yDisplacement = globalDisplacementVector[2 * i + 1];
        }
        //Adding the reaction forces for showing them
        double[] erovektor = new double[globalForcesVector.Length];
        double[] kulsoerok = MergeForceVectors();
        for (int i = 0; i < globalForcesVector.Length; i++)
        {
            erovektor[i] = globalForcesVector[i] - kulsoerok[i];
        }
        for (int i = 0; i < nodes.Count; i++)
        {
            //getting rid of the numerical errors
            if (Math.Abs(erovektor[2 * i]) > 0.5 || Math.Abs(erovektor[2 * i + 1]) > 0.5)
            {
                Force2D force = new Force2D(nodes[i], erovektor[2 * i], erovektor[2 * i + 1], ForceType.Reaction);
                forces.Add(force);
            }
        }

        //calculate the stress and strain
        for (int i = 0; i < trusses.Count; i++)
        {
            TrussElement2D element = trusses[i];
            Node2D node1 = element.Node1;
            Node2D node2 = element.Node2;

            // original length vector scalar pruduct with the deformation vector = length*deformationlength*cos(theta), where theta is the angle between the two vectors, thets why we divide by length^2
            //element.Strain = ((node2.xDisplacement - node1.xDisplacement) * (node2.X - node1.X) + (node2.yDisplacement - node1.yDisplacement) * (node2.Y - node1.Y)) / (element.Length * element.Length);
            element.Strain = ((node2.X-node1.X)/element.Length*(node2.xDisplacement-node1.xDisplacement)
                +(node2.Y-node1.Y)/element.Length*(node2.yDisplacement-node1.yDisplacement))/element.Length; //ugyan az a ketto

            element.Stress = element.YoungsModulus * element.Strain;
        }
    }

    public double[] MergeForceVectors() // Merging all the forces into a single vector
    {
        int numNodes = nodes.Count;
        int globalSize = numNodes * 2;

        double[] globalForceVector = new double[globalSize];

        foreach (Force2D force in forces)
        {
            Node2D node = force.node;
            int index = nodes.IndexOf(node);
            globalForceVector[index * 2] += force.XForce;
            globalForceVector[index * 2 + 1] += force.YForce;
        }

        return globalForceVector;
    }
    public double[,] CalculateStiffnessMatrix(TrussElement2D element)
    {
        // Calculate the length of the truss element
        double length = (double)Math.Sqrt(Math.Pow(element.Node1.X - element.Node2.X, 2) + Math.Pow(element.Node2.Y - element.Node1.Y, 2));

        // Calculate the cosine and sine of the element's orientation angle
        double cosTheta = (element.Node2.X - element.Node1.X) / length;
        double sinTheta = (element.Node2.Y - element.Node1.Y) / length;

        // Calculate the stiffness matrix components
        double k = element.YoungsModulus * element.CrossSectionalArea / length;
        double c2 = k * cosTheta * cosTheta;
        double sc = k * cosTheta * sinTheta;
        double s2 = k * sinTheta * sinTheta;

        // Create the stiffness matrix
        double[,] stiffnessMatrix = new double[4, 4];
        stiffnessMatrix[0, 0] = c2;
        stiffnessMatrix[0, 1] = sc;
        stiffnessMatrix[0, 2] = -c2;
        stiffnessMatrix[0, 3] = -sc;
        stiffnessMatrix[1, 0] = sc;
        stiffnessMatrix[1, 1] = s2;
        stiffnessMatrix[1, 2] = -sc;
        stiffnessMatrix[1, 3] = -s2;
        stiffnessMatrix[2, 0] = -c2;
        stiffnessMatrix[2, 1] = -sc;
        stiffnessMatrix[2, 2] = c2;
        stiffnessMatrix[2, 3] = sc;
        stiffnessMatrix[3, 0] = -sc;
        stiffnessMatrix[3, 1] = -s2;
        stiffnessMatrix[3, 2] = sc;
        stiffnessMatrix[3, 3] = s2;


        return stiffnessMatrix;
    }
    public double[,] MergeStiffnessMatrices() // Merging all the stiffness matrices into a single matrix
    {
        int numElements = trusses.Count;
        int numNodes = nodes.Count;
        int globalSize = numNodes * 2;

        double[,] globalStiffnessMatrix = new double[globalSize, globalSize];

        foreach (TrussElement2D element in trusses)
        {
            Node2D node1 = element.Node1;
            Node2D node2 = element.Node2;

            int index1 = nodes.IndexOf(node1);
            int index2 = nodes.IndexOf(node2);

            double[,] elementStiffnessMatrix = CalculateStiffnessMatrix(element);
            for (int j = 0; j < 2; j++)
            {
                for (int k = 0; k < 2; k++)
                {
                    globalStiffnessMatrix[index1 * 2 + j, index1 * 2 + k] += elementStiffnessMatrix[j, k];
                    globalStiffnessMatrix[index1 * 2 + j, index2 * 2 + k] += elementStiffnessMatrix[j, k + 2];
                    globalStiffnessMatrix[index2 * 2 + j, index1 * 2 + k] += elementStiffnessMatrix[j + 2, k];
                    globalStiffnessMatrix[index2 * 2 + j, index2 * 2 + k] += elementStiffnessMatrix[j + 2, k + 2];
                }
            }
        }
        return globalStiffnessMatrix;
    }
    public double[,] CondenseStiffnessMatrix(double[,] globalStiffnessMatrix)
    {
        int numNodes = nodes.Count;
        int globalSize = numNodes * 2;

        bool[] fixedDOFs = FixedDofs();

        // Calculate the size of the condensed stiffness matrix
        int condensedSize = 0;
        for (int i = 0; i < globalSize; i++)
        {
            if (!fixedDOFs[i])
            {
                condensedSize++;
            }
        }

        double[,] condensedStiffnessMatrix = new double[condensedSize, condensedSize];
        int condensedRow = 0;
        int condensedCol = 0;
        // Condensating the stiffness matrix
        for (int row = 0; row < globalSize; row++)
        {
            if (!fixedDOFs[row])
            {
                for (int col = 0; col < globalSize; col++)
                {
                    if (!fixedDOFs[col])
                    {
                        condensedStiffnessMatrix[condensedRow, condensedCol] = globalStiffnessMatrix[row, col];
                        condensedCol++;
                    }
                }
                condensedRow++;
                condensedCol = 0;
            }
        }

        return condensedStiffnessMatrix;
    }
    public double[] CondenseForceVector(double[] globalForceVector)
    {
        int numNodes = nodes.Count;
        int globalSize = numNodes * 2;

        bool[] fixedDOFs = FixedDofs();

        // Calculate the size of the condensed force vector
        int condensedSize = 0;
        for (int i = 0; i < globalSize; i++)
        {
            if (!fixedDOFs[i])
            {
                condensedSize++;
            }
        }

        double[] condensedForceVector = new double[condensedSize];
        int condensedIndex = 0;
        // Condensating the force vector
        for (int i = 0; i < globalSize; i++)
        {
            if (!fixedDOFs[i])
            {
                condensedForceVector[condensedIndex] = globalForceVector[i];
                condensedIndex++;
            }
        }

        return condensedForceVector;
    }
    public double[] SolveLinearSystem(double[,] matrix, double[] vector) // Solving the 
    {
        // Solve the linear system using L U conversion
        double[,] L = null;
        double[,] U = null;
        LUdcmpMethod(matrix, ref L, ref U);
        double[] y = FwdSubstitution(L, vector);
        double[] x = BwdSubstitution(U, y);

        return x;
    }

    //matrix inversion functions (old way of solving the linear system)
    /*private double[,] InvertMatrix(double[,] matrix)
    {
        int size = matrix.GetLength(0);
        double[,] inverseMatrix = new double[size, size];

        // Calculate the determinant of the matrix
        double determinant = CalculateDeterminant(matrix);

        // Check if the matrix is invertible
        if (determinant == 0)
        {
            //throw new System.DivideByZeroException("Matrix is not invertible");
            return null;
        }

        // Calculate the adjugate matrix
        double[,] adjugateMatrix = CalculateAdjugateMatrix(matrix);

        // Calculate the inverse matrix
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                inverseMatrix[i, j] = adjugateMatrix[i, j] / determinant;
            }
        }

        return inverseMatrix;
    }

    private double CalculateDeterminant(double[,] matrix)
    {
        int size = matrix.GetLength(0);

        // Base case for 2x2 matrix
        if (size == 2)
        {
            return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
        }

        double determinant = 0;

        // Calculate the determinant using cofactor expansion
        for (int j = 0; j < size; j++)
        {
            double cofactor = matrix[0, j] * CalculateCofactor(matrix, 0, j);
            determinant += cofactor;
        }

        return determinant;
    }

    private double CalculateCofactor(double[,] matrix, int row, int col)
    {
        int size = matrix.GetLength(0);
        double[,] submatrix = new double[size - 1, size - 1];

        int subRow = 0;
        int subCol = 0;

        // Create the submatrix by excluding the specified row and column
        for (int i = 0; i < size; i++)
        {
            if (i != row)
            {
                for (int j = 0; j < size; j++)
                {
                    if (j != col)
                    {
                        submatrix[subRow, subCol] = matrix[i, j];
                        subCol++;
                    }
                }
                subRow++;
                subCol = 0;
            }
        }

        // Calculate the cofactor using the submatrix
        double cofactor = CalculateDeterminant(submatrix) * (double)Math.Pow(-1, row + col);

        return cofactor;
    }

    private double[,] CalculateAdjugateMatrix(double[,] matrix)
    {
        int size = matrix.GetLength(0);
        double[,] adjugateMatrix = new double[size, size];

        // Calculate the cofactor matrix
        for (int i = 0; i < size; i++)
        {
            for (int j = 0; j < size; j++)
            {
                adjugateMatrix[i, j] = CalculateCofactor(matrix, j, i); // Transpose the cofactor matrix
            }
        }

        return adjugateMatrix;
    }*/
    public double[] MultiplyMatrixVector(double[,] matrix, double[] vector)
    {
        int numRows = matrix.GetLength(0);
        int numCols = matrix.GetLength(1);

        if (numCols != vector.Length)
        {
            throw new System.ArgumentException("Matrix and vector dimensions are not compatible for multiplication");
        }

        double[] result = new double[numRows];

        for (int i = 0; i < numRows; i++)
        {
            for (int j = 0; j < numCols; j++)
            {
                result[i] += matrix[i, j] * vector[j];
            }
        }

        return result;
    }
    public double[] GetFullDisplacementVector(double[] condensedDisplacementVector)
    {
        int numNodes = nodes.Count;
        int globalSize = numNodes * 2;

        bool[] fixedDOFs = FixedDofs();

        double[] fullDisplacementVector = new double[globalSize];
        int condensedIndex = 0;
        // Expand the condensed displacement vector to the full displacement vector
        for (int i = 0; i < globalSize; i++)
        {
            if (!fixedDOFs[i])
            {
                fullDisplacementVector[i] = condensedDisplacementVector[condensedIndex];
                condensedIndex++;
            }
            else
            {
                fullDisplacementVector[i] = 0f;
            }
        }
        return fullDisplacementVector;
    }
    private bool[] FixedDofs()
    {
        int globalSize = nodes.Count * 2;
        bool[] fixedDOFs = new bool[globalSize];
        foreach (Node2D node in nodes)
        {
            if (node.boundaryCondition == BoundaryCondition.Pinned)
            {
                fixedDOFs[nodes.IndexOf(node) * 2] = true;
                fixedDOFs[nodes.IndexOf(node) * 2 + 1] = true;
            }
            else if (node.boundaryCondition == BoundaryCondition.xRoller)
            {
                fixedDOFs[nodes.IndexOf(node) * 2 + 1] = true;
            }
            else if (node.boundaryCondition == BoundaryCondition.yRoller)
            {
                fixedDOFs[nodes.IndexOf(node) * 2] = true;
            }
        }
        return fixedDOFs;
    }

}
public interface IInstantiable
{
    GameObject CreateInstance(GameObject gameObject, float magnification,float z);
    string DataToString();
    protected static string RoundToThreeSignificantFigures(double number)
    {
        if (number == 0)
        {
            return 0.ToString();
        }

        double magnitude = Math.Pow(10, Math.Floor(Math.Log10(Math.Abs(number))) + 1);
        double roundedNumber = Math.Round(number / magnitude, 3) * magnitude;

        return roundedNumber.ToString();
    }
}
public enum BoundaryCondition
{
    Pinned,
    yRoller,
    xRoller,
    Free
}
public enum ForceType
{
    Outer,
    Reaction
}
public class Node2D:IInstantiable
{
    public BoundaryCondition boundaryCondition { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double xDisplacement { get; set; }
    public double yDisplacement { get; set; }
    public Vector3 displacement
    {
        get
        {
            return new Vector3((float)xDisplacement, (float)yDisplacement, 0);
        }
    }
    public Node2D(double x, double y)
    {
        X = x;
        Y = y;
    }
    public Vector3 position()
    {
        return new Vector3((float)X, (float)Y, 0);
    }
    public Vector3 position(float z)
    {
  
        return new Vector3((float)X, (float)Y, z);
    }

    public GameObject CreateInstance(GameObject gameObject, float magnification,float z)
    {
        GameObject instance= null;
        switch (boundaryCondition)
        {
            case BoundaryCondition.Pinned:
                instance = GameObject.Instantiate(gameObject, position(z) + magnification * displacement, Quaternion.identity);
                break;
            case BoundaryCondition.xRoller:
                instance = GameObject.Instantiate(gameObject, position(z) + magnification * displacement, Quaternion.identity);
                break;
            case BoundaryCondition.yRoller:
                instance = GameObject.Instantiate(gameObject, position(z) + magnification * displacement, Quaternion.Euler(0, 0, 90));
                break;
            case BoundaryCondition.Free:
                instance = GameObject.Instantiate(gameObject, position(z) + magnification * displacement, Quaternion.identity);
                break;
        }
        instance.GetComponent<BuildingBlock>().SetTag("result");
        instance.GetComponent<BuildingBlock>().information = DataToString();
        return instance;
    }
    public string DataToString()
    {
        if (boundaryCondition == BoundaryCondition.Pinned)
        {
            return "Node at (" + IInstantiable.RoundToThreeSignificantFigures(X) + ", " + IInstantiable.RoundToThreeSignificantFigures(Y) +
            ")\n All coordinates in [m]\nBoundary condition: " + boundaryCondition.ToString();
        }
        return "Node at (" + IInstantiable.RoundToThreeSignificantFigures(X) + ", " + IInstantiable.RoundToThreeSignificantFigures(Y) + 
            ")\n All coordinates in [m]\nBoundary condition: "+boundaryCondition.ToString() +
            "\nDisplacement:\nx: "+ IInstantiable.RoundToThreeSignificantFigures(xDisplacement) + "[m]\ny: "+ IInstantiable.RoundToThreeSignificantFigures(yDisplacement) + "[m]";
    }
}
public class Force2D:IInstantiable
{
    public ForceType forceType { get; set; }
    public Node2D node { get; set; }
    public double strength { get; set; }
    private double _XForce = 0;
    public double XForce { 
        get
        {
            return _XForce;
        } 
        set{
            _XForce = value;
            strength = (double)Math.Sqrt(_XForce * _XForce + _YForce * _YForce);
        }
    }
    private double _YForce = 0;
    public double YForce
    {
        get
        {
            return _YForce;
        }
        set
        {
            _YForce = value;
            strength = (double)Math.Sqrt(_XForce * _XForce + _YForce * _YForce);
        }
    }

    public Force2D(Node2D node, double xForce, double yForce, ForceType type)
    {
        if (xForce == 0 && yForce == 0)
        {
            throw new System.ArgumentException("Force must have a non-zero magnitude");
        }
        this.node = node;
        XForce = xForce;
        YForce = yForce;
        forceType = type;
    }
    private Vector3 direction
    {
        get
        {
            return new Vector3((float)XForce, (float)YForce, 0);
        }
    }
    public GameObject CreateInstance(GameObject gameObject, float magnification, float z)
    {
        GameObject instance = GameObject.Instantiate(gameObject, node.position(z) + magnification * node.displacement, Quaternion.LookRotation(direction));
        instance.GetComponent<BuildingBlock>().SetTag("result");
        instance.GetComponent<Force>().SetMagnitude((float)strength / 10000.0f);
        if (forceType == ForceType.Reaction)
        {
            instance.GetComponent<Force>().SetMaterial(Resources.Load<Material>("Materials/forceReaction"));
        }
        instance.GetComponent<BuildingBlock>().information = DataToString();
        return instance;
    }
    public string DataToString()
    {
        return "Force magnitude: " + IInstantiable.RoundToThreeSignificantFigures(strength)+" [N]"+ "\nX component: " + IInstantiable.RoundToThreeSignificantFigures(XForce) + " [N]" + "\nY component: " + IInstantiable.RoundToThreeSignificantFigures(YForce) + " [N]";
    }
}
public class TrussElement2D:IInstantiable
{
    public Node2D Node1 { get; set; }
    public Node2D Node2 { get; set; }
    public double YoungsModulus { get; set; } = 210e9; // Young's modulus of steel in Pascals
    public double CrossSectionalArea { get; set; } = 0.00001; // Cross-sectional area of the truss element in square meters

    public double Length { get; private set; }
    public double Stress { get; set; }
    public double Strain { get; set; }

    protected GameObject TrussVR;

    public TrussElement2D(Node2D node1, Node2D node2)
    {
        Node1 = node1;
        Node2 = node2;
        Length = (double)Math.Sqrt(Math.Pow(Node2.X - Node1.X, 2) + Math.Pow(Node2.Y - Node1.Y, 2));
    }
    public TrussElement2D(Node2D node1, Node2D node2, double youngsModulus, double crossSectionArea)
    {
        Node1 = node1;
        Node2 = node2;
        YoungsModulus = youngsModulus;
        CrossSectionalArea = crossSectionArea;
        Length = (double)Math.Sqrt(Math.Pow(Node2.X - Node1.X, 2) + Math.Pow(Node2.Y - Node1.Y, 2));
    }
    public GameObject CreateInstance(GameObject gameObject, float magnification, float z)
    {
        GameObject instance = GameObject.Instantiate(gameObject, Node1.position(z) + magnification * Node1.displacement, Quaternion.LookRotation(Node2.position() + magnification * Node2.displacement - Node1.position() - magnification * Node1.displacement));
        instance.transform.localScale = new Vector3(1, 1, (float)Vector3.Distance(Node1.position()+ magnification * Node1.displacement, Node2.position() + magnification * Node2.displacement));
        instance.GetComponent<BuildingBlock>().SetTag("result");
        instance.GetComponent<BuildingBlock>().information = DataToString();
        TrussVR = instance;
        return instance;
    }
    public void ShowStress(double minimum, double variance)
    {
        //the used material is from the resources folder, that can be found in the assets foldel.
        Material stressMaterial = new Material(Resources.Load<Material>("Materials/FeherStresshez"));
        stressMaterial.color = Color.HSVToRGB((float)((Stress - minimum) / variance * 0.64), 1, 1);

        TrussVR.GetComponent<BuildingBlock>().SetMaterial(stressMaterial);
    }
    public void ShowAbsoluteStress(double max)
    {
        Material stressMaterial = new Material(Resources.Load<Material>("Materials/FeherStresshez"));
        stressMaterial.color = Color.HSVToRGB((1-(float)Math.Abs(Stress/max)) * 0.32f, 1, 1);

        TrussVR.GetComponent<BuildingBlock>().SetMaterial(stressMaterial);
    }
    public void EndShowStress()
    {
        TrussVR.GetComponent<BuildingBlock>().SetOriginalMaterial();
    }
    public string DataToString()
    {
        return "Truss element length: "+IInstantiable.RoundToThreeSignificantFigures(Length) +" [m]"+
            "\nCrossection area: " + CrossSectionalArea + 
            " [m^2]\nYoung's modulus: " + YoungsModulus / 1000000000 + " [GPa]"+
            "\nStrain: " + IInstantiable.RoundToThreeSignificantFigures(Strain)+
            "\nStress: " + IInstantiable.RoundToThreeSignificantFigures(Stress/1000000) + " [MPa]";
    }
}
