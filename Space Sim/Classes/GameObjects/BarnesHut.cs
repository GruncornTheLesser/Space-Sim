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
     * Calculate Forces
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

    public class BarnesHut : IEnumerable<PointMass>
    {
        internal List<PointMass> MassPool = new List<PointMass>();
        internal Stack<Leaf> LeafPool = new Stack<Leaf>();
        internal Quad TotalQuad;
        Branch Root;

        public BarnesHut(Vector2 Min, Vector2 Max)
        {
            TotalQuad = new Quad(Min, Max);
            Root = new Branch(this, null, TotalQuad);
        }

        internal void Evolve(float delta, float theta)
        {
            // recreate tree
            Clear();
            foreach (Leaf L in LeafPool) Root.Insert(L);
            foreach (Leaf L in LeafPool)
            {
                L.PointMass.Velocity += Root.AccelForPointMass(L.PointMass, theta) * delta;
            }
        }
        internal void Clear()
        {
            Root = new Branch(this, null, TotalQuad);
        }
        internal void Add(PointMass P)
        {
            MassPool.Add(P); // just for brute force algorithm
            LeafPool.Push(new Leaf(this, P));
        }

        // used for brute force algorithms
        IEnumerator<PointMass> IEnumerable<PointMass>.GetEnumerator()
        {
            foreach (Leaf L in LeafPool) yield return L.PointMass;
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            foreach (Leaf L in LeafPool) yield return L.PointMass;
        }

        internal abstract class SubTree
        {
            internal Branch Parent;
            internal Quad Quad;
            internal BarnesHut Tree;

            public SubTree(BarnesHut Tree, Branch Parent, Quad Quad)
            {
                this.Tree = Tree;
                this.Parent = Parent;
                this.Quad = Quad;
            }
            /// <summary>
            /// Calculates the acceleration from this point mass to move point mass P
            /// </summary>
            /// <param name="P">the point mass that's acceleration is being calculated</param>
            /// <returns>the acceleration from the force of this object</returns>
            internal abstract Vector2d AccelForPointMass(PointMass P, float theta);
        }



        internal class Branch : SubTree
        {
            internal Quad[] Quads = new Quad[4];
            internal SubTree[] Children = new SubTree[4];

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
            internal void Insert(Leaf Leaf)
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
            internal override Vector2d AccelForPointMass(PointMass P, float theta)
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



        internal class Leaf : SubTree
        {
            internal PointMass PointMass;
            internal Leaf(BarnesHut Tree, PointMass P) : base(Tree, null, new Quad()) => PointMass = P;// on init, Parent nor quad is set yet
            internal override Vector2d AccelForPointMass(PointMass P, float theta)
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
            internal Vector2 Min, Max;
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

