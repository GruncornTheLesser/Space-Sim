using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;

namespace GameObjects
{
    /* 
     * To calculate net force on a particle, the tree is traversed from the root.
     * 
     * If the Center of Mass is far enough away approximate those bodies as a single body.
     * else traverse each of its children and recursively chick those.
     * 
     * A node branch is sufficiently far away if s / d < θ 
     * where s is the width of the current branch
     * where d is the distance between the body and the center of mass. 
     * where θ is a value we use to control the accuracy of the simulation. 
     * if θ = 0 algorithm degenerates to brute force.
     * if θ = 1 algorithm calculates only from the center of masses of the root.
     * 
     * Construct Tree
     * 
     * iterate through each body and insert them into the tree root use:
     *  
     * to insert body B into SubTree S
     * if S is null, add B to S, set center of mass
     * if S is Branch, insert B into Branch, update center of mass
     * if S is Leaf, Create new branch using S Quad, insert S, insert B, update center of mass
     * 
     * Calculate Forces/Accelerations
     * 
     * iterate through each body and starting at the root traverse the tree use:
     * to find force on body B from SubTree S:
     * if S is null continue
     * if S is Leaf and S is not B, add Calcuated Acceleration due to S
     * if S is Branch,
     *      if s/d ratio < θ, treat Branch as a single body, add Calculated Acceleration due to S
     *      else if s/d ratio > θ. find force on body B from children of S.
     * 
     */

    public class BarnesHut : IEnumerable
    {
        private Stack<Leaf> LeafPool = new Stack<Leaf>();
        private Quad TQuad; // max quad
        Branch Root;

        public BarnesHut(Vector2 Min, Vector2 Max)
        {
            TQuad = new Quad(Min, Max);
            Root = new Branch(this, null, TQuad);
        }

        internal void Evolve(float delta, float theta)
        {
            
            Clear();
            // recreate tree
            foreach (Leaf L in LeafPool) Root.Insert(L);
            // calculate acceleration and change velocity
            foreach (Leaf L in LeafPool) L.PointMass.Velocity += Root.AccelForPointMass(L.PointMass, theta) * delta; 

        }
        /// <summary>
        /// adds pointmass P to pool. when Tree is next evolved, P will be incorportored. however you spell that.
        /// </summary>
        /// <param name="P">pointmass to be added.</param>
        internal void Add(PointMass P) => LeafPool.Push(new Leaf(this, P));
        /// <summary>
        /// clears the tree.
        /// </summary>
        private void Clear() => Root = new Branch(this, null, TQuad);
        
        IEnumerator IEnumerable.GetEnumerator()
        {
            // used in brute force algorithm
            foreach (Leaf L in LeafPool) yield return L.PointMass;// iterates through Point Masses
        }
        
        /// <summary>
        /// abstract class containing the key fields for nodes in the tree.
        /// </summary>
        internal abstract class SubTree
        {
            public Branch Parent;
            public Quad Quad;
            public BarnesHut Tree;

            public SubTree(BarnesHut Tree, Branch Parent, Quad Quad)
            {
                this.Tree = Tree;
                this.Parent = Parent;
                this.Quad = Quad;
            }
            /// <summary>
            /// Calculates the acceleration from this point mass to move point mass P
            /// </summary>
            /// <param name="P">the pointmass thats being accelerated</param>
            /// <param name="theta">a value etwee 1 and 0 to control the accuracy of the simulation.</param>
            /// <returns>the acceleration from the force of this point mass of cluster of point masses</returns>
            public abstract Vector2d AccelForPointMass(PointMass P, float theta);
        }


        /// <summary>
        /// a node in the tree which has children
        /// </summary>
        internal class Branch : SubTree
        {
            internal Quad[] Quads = new Quad[4];
            internal SubTree[] Children = new SubTree[4]; // array of leaves and branches

            internal int Count = 0;
            internal double TotalMass;
            internal Vector2 TotalPosition;

            public Branch(BarnesHut Tree, Branch Parent, Quad Quad) : base(Tree, Parent, Quad)
            {

                Vector2 Mid = Quad.Min + (Quad.Max - Quad.Min) * 0.5f;
                // create new quads 
                Quads[0] = new Quad(Quad.Min, Mid);
                Quads[1] = new Quad(Quad.Min.X, Mid.Y, Mid.X, Quad.Max.Y);
                Quads[2] = new Quad(Mid, Quad.Max);
                Quads[3] = new Quad(Mid.X, Quad.Min.Y, Quad.Max.X, Mid.Y);
            }
            public void Insert(Leaf Leaf)
            {
                Count += 1;
                TotalMass += Leaf.PointMass.Mass;
                TotalPosition += Leaf.PointMass.Position;
                for (int i = 0; i < 4; i++)
                {
                    if (Quads[i].Contains(Leaf.PointMass.Position))
                    {
                        switch (Children[i])
                        {
                            case null:
                                // if null set to leaf
                                Leaf.Parent = this; // parent set because its a inserting directly to this object
                                Children[i] = Leaf;
                                return;

                            case Branch _:
                                // if branch insert leaf into child branch
                                ((Branch)Children[i]).Insert(Leaf);
                                return;

                            case Leaf _:
                                // If leaf convert to branch and insert new leaf into new child branch
                                Branch B = new Branch(Tree, Children[i].Parent, Quads[i]);
                                B.Insert((Leaf)Children[i]); // re-insert old leaf into new Branch
                                B.Insert(Leaf); // insert new leaf
                                Children[i] = B;
                                return;
                        }
                    }
                }
            }
            public override Vector2d AccelForPointMass(PointMass P, float theta)
            {
                Vector2d Acc = Vector2.Zero;
                float s = Quad.Max.X - Quad.Min.X;
                float d = (P.Position - (TotalPosition / Count)).Length;
                if (s / d > theta)
                {
                    foreach (SubTree S in Children)
                    {
                        if (S != null)
                        {
                            Acc += S.AccelForPointMass(P, theta);
                        }
                    }
                }
                else 
                { 
                    Acc = P.CalcAccFrom(TotalMass, TotalPosition / Count); 
                }
                return Acc;
            }
        }


        /// <summary>
        /// The end of a branch. mostly just a wrapper of a Pointmass.
        /// </summary>
        internal class Leaf : SubTree
        {
            public PointMass PointMass;
            public Leaf(BarnesHut Tree, PointMass P) : base(Tree, null, new Quad()) => PointMass = P;// on init, Parent nor quad is set yet
            public override Vector2d AccelForPointMass(PointMass P, float theta)
            {
                if (PointMass == P) return Vector2d.Zero;
                else return P.CalcAccFrom(PointMass.Mass, PointMass.Position);
            }
        }
        
        
        /// <summary>
        /// represents a quad defined by the bottom left corner and the top right corner.
        /// </summary>
        internal struct Quad
        {
            public Vector2 Min, Max;
            public Quad(Vector2 Min, Vector2 Max)
            {
                this.Min = Min;
                this.Max = Max;
            }
            public Quad(float MinX, float MinY, float MaxX, float MaxY) : this(new Vector2(MinX, MinY), new Vector2(MaxX, MaxY)) { }
            public bool Intersects(Quad that) => Min.X <= that.Max.X && Min.Y <= that.Max.Y && Max.X > that.Min.X && Max.Y > that.Min.Y;
            public bool Contains(Quad that) => that.Min.X >= Min.X && that.Min.Y >= Min.Y && that.Max.X <= Max.X && that.Max.Y <= Max.Y;
            public bool Contains(Vector2 Pos) => Pos.X >= Min.X && Pos.Y >= Min.Y && Pos.X < Max.X && Pos.Y < Max.Y;
            public override string ToString() => $"Min: {Min} Max: {Max}";
        }
    }
}

