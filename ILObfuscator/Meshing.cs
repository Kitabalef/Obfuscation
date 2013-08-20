﻿//#define PSEUDOCODE

#if !PSEUDOCODE

/*
 * So this is the algorithm for meshing the control flow transition blocks.
 * This algorithm can be divided into two main parts:
 *  - Meshing the unconditional jumps
 *  - Meshing the conditional jumps
 * 
 * The complexity of the final CFG is based on the CFT Ratio, or Control Flow Transition
 * Ratio, which is the basic parameter of this algorithm of the obfuscaion. Depending on
 * this, the run time of the obfuscated part obviously slow down, so further considerations
 * must be done to set the default value of the CFT Ratio.
 * 
 * First of all, the algorithm needs a set of jumps -- conditional and unconditional also
 * -- on which the meshing will be terminated. The setting of this set is the first step,
 * because if we mesh up the CFG based on the unconditional jumps, and we try to determine
 * the conditional jumps which will locate the place of the meshing by the conditional
 * jumps, we might face the situation that for instance we only mesh up the conditional
 * jumps which are generated by us erstwhile, the original conditional jumps will remain
 * unmeshed... this explanation is getting to be too verbosed, so the point is: the first
 * step is to define the several jumps which the mesh will be based on.
 */

// Dmitriy: Agree on that, but we can easily solve it. Let's discuss it on Friday!


/*
 * This is the main function, it meshes up the single conditional and
 * unconditional jumps.
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Internal;

namespace Obfuscator
{
    public static class Meshing
    {

        /// <summary>
        /// The meshing algorithm, which will mesh the CFT-s in a fuction
        /// </summary>
        /// <param name="funct">Thse function which has the control flow transitions to be meshed up</param>
        public static void MeshFunction( Function funct )
        {
            /// Meshing of the unconditional jumps
            List<BasicBlock> basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eUnconditionalJump);
            foreach (BasicBlock bb in basicblocks)
            {
                InsertFakeLane(bb);
                InsertDeadLane(bb);
            }

            /// Meshing of the conditional jumps
            basicblocks = funct.BasicBlocks.FindAll(x => x.Instructions.Last().statementType == ExchangeFormat.StatementTypeType.EnumValues.eConditionalJump);
            foreach (BasicBlock bb in basicblocks)
            {
                //MeshConditionals(bb);
            }
        }

        /// <summary>
        /// Inserts the fake lane into the CFT
        /// Still in test phase, now it only has a not so smart condition in the conditional jump
        /// </summary>
        /// <param name="bb">The actual basic block with the unconditional jump</param>
        private static void InsertFakeLane(BasicBlock bb)
        {
            // Saving the original target of the jump
            BasicBlock originaltarget = bb.getSuccessors.First();

            // Creating a new basic block after "bb"
            BasicBlock fake1 = bb.SplitAfterInstruction(bb.Instructions.Last());

            // Creating the second fake block, after fake1
            BasicBlock fake2 = fake1.SplitAfterInstruction(fake1.Instructions.Last());

            //Creating the third one, after fake2
            BasicBlock fake3 = fake2.SplitAfterInstruction(fake2.Instructions.Last());

            // Creating a clone of the original target in order to make the CFT more obfuscated
            //BasicBlock polyrequtarget = originaltarget.Clone(true);
            
            // ************ Andris! Please, check if it is what you wanted to do with Clone(...)
            BasicBlock polyrequtarget = Obfuscator.Common.DeepClone(originaltarget) as BasicBlock;
            polyrequtarget.Instructions.ForEach(delegate(Instruction inst) { inst.polyRequired = true; });
            // ************ Please, check successor-predecessor links that you are setting below, maybe there is a problem...
            // ************ If you need method for setting predecessors (like LinkToSuccessors), please let me know

            // And now setting the edges
            fake2.LinkToSuccessor(polyrequtarget, true);

            // And then converting its nop instruction into a ConditionalJump, and by that we create a new block

            // ************* Please, use MakeConditionalJumpInstruction(...) method from Randomizer class
            fake1.Instructions.First().MakeConditionalJump(fake1.parent.LocalVariables[ Randomizer.GetSingleNumber( 0, fake1.parent.LocalVariables.Count-1)], Randomizer.GetSingleNumber(0, 100), (Instruction.RelationalOperationType) Randomizer.GetSingleNumber(0,5), fake3);

            // It creates the fake lane, but the condition is not a smart one yet, it is a ~random~ condition.
            // TODO: Creating smart conditions
            //       Test for mistakes in polyrequied cloning and linking
            
        }

        /// <summary>
        /// Inserts the dead lane into the CFT and also changing the original unconditional jump into a conditional
        /// In thest phase -> condition not always false
        /// </summary>
        /// <param name="bb">The actual basic block with the unconditional jump</param>
        private static void InsertDeadLane(BasicBlock bb)
        {

            // First, creating a basic block pointing to a random block of the function
            BasicBlock dead1 = new BasicBlock(bb.parent);

            // And making it dead
            dead1.dead = true;

            // Now including another one, with the basicblock splitter
            BasicBlock dead2 = dead1.SplitAfterInstruction(dead1.Instructions.Last());

            // And making it dead too
            dead2.dead = true;

            // Now including the third one, with the basicblock splitter
            BasicBlock dead3 = dead2.SplitAfterInstruction(dead2.Instructions.Last());

            // And making it dead too
            dead3.dead = true;

            // Now creating the conditional jump
            dead1.Instructions.Last().MakeConditionalJump(bb.parent.LocalVariables[Randomizer.GetSingleNumber(0, bb.parent.LocalVariables.Count - 1)], Randomizer.GetSingleNumber(0, 100), Instruction.RelationalOperationType.Smaller, dead3);

            // Here comes the tricky part: changing the bb's unconditional jump to a conditional, which is always false
            bb.Instructions.Last().ConvertUncondToCondJump(bb.parent.LocalVariables[Randomizer.GetSingleNumber(0, bb.parent.LocalVariables.Count - 1)], Randomizer.GetSingleNumber(0, 100), Instruction.RelationalOperationType.Smaller, dead1);

            // And finally we link the remaining blocks
            dead2.LinkToSuccessor(Randomizer.GetJumpableBasicBlock(bb.parent), true);
            dead3.LinkToSuccessor(Randomizer.GetJumpableBasicBlock(bb.parent), true);

        }

        /// <summary>
        /// Class of constants with the relational operations available list
        /// </summary>
        internal class Cond
        {
            public enum BlockJumpType
            {
                True,
                False,
                Ambigious,
                Last
            }

            public int value;

            public Instruction.RelationalOperationType relop;

            public BlockJumpType JumpType;

            /// <summary>
            /// Constructor to the condition class
            /// </summary>
            /// <param name="generatedconstant">The generated constant of the condition</param>
            /// <param name="originalrelop">The original relational operator</param>
            /// <param name="originalconstant">The original constant</param>
            public Cond(int generatedconstant, Instruction.RelationalOperationType originalrelop, int originalconstant)
            {
                value = generatedconstant;
                SetJumpType(generatedconstant, originalconstant, originalrelop);
            }

            /// <summary>
            /// Sets the jump type of a Cond
            /// </summary>
            /// <param name="generatedconstant">The constant which has been generated by us</param>
            /// <param name="originalconstant">The original constant</param>
            /// <param name="originalrelop">The original relational operator</param>
            private void SetJumpType(int generatedconstant, int originalconstant, Instruction.RelationalOperationType originalrelop)
            {
                if (generatedconstant == originalconstant - 1)
                {
                    switch (originalrelop)
                    {
                        case Instruction.RelationalOperationType.GreaterOrEquals:
                            relop = Instruction.RelationalOperationType.Greater;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.Smaller:
                            relop = Instruction.RelationalOperationType.SmallerOrEquals;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.Equals:
                            relop = Instruction.RelationalOperationType.Greater;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.NotEquals:
                            relop = Instruction.RelationalOperationType.Smaller;
                            JumpType = BlockJumpType.Last;
                            break;
                        default:
                            LessPattern(originalrelop);
                            break;
                    }
                }
                else if (generatedconstant == originalconstant + 1)
                {
                    switch (originalrelop)
                    {
                        case Instruction.RelationalOperationType.Greater:
                            relop = Instruction.RelationalOperationType.GreaterOrEquals;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.SmallerOrEquals:
                            relop = Instruction.RelationalOperationType.Smaller;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.Equals:
                            relop = Instruction.RelationalOperationType.Smaller;
                            JumpType = BlockJumpType.Last;
                            break;
                        case Instruction.RelationalOperationType.NotEquals:
                            relop = Instruction.RelationalOperationType.Greater;
                            JumpType = BlockJumpType.Last;
                            break;
                        default:
                            GreaterPattern(originalrelop);
                            break;
                    }
                }
                else if (generatedconstant < originalconstant)
                {
                    LessPattern(originalrelop);
                }
                else
                {
                    GreaterPattern(originalrelop);
                }
            }

            /// <summary>
            /// Sets the jump type of the Cond, based on the original relational operator, and a pattern
            /// </summary>
            /// <param name="originalrelop">The original relational operator</param>
            private void GreaterPattern(Instruction.RelationalOperationType originalrelop)
            {
                relop = Randomizer.GetRelop();
                if (relop == Instruction.RelationalOperationType.Greater || relop == Instruction.RelationalOperationType.GreaterOrEquals || relop == Instruction.RelationalOperationType.Equals)
                {
                    if (originalrelop == Instruction.RelationalOperationType.Greater || originalrelop == Instruction.RelationalOperationType.GreaterOrEquals || originalrelop == Instruction.RelationalOperationType.NotEquals)
                        JumpType = BlockJumpType.True;
                    else JumpType = BlockJumpType.False;
                }
                else JumpType = BlockJumpType.Ambigious;
            }

            /// <summary>
            /// Sets the jump type of the Cond, based on the original relational operator, and a pattern
            /// </summary>
            /// <param name="originalrelop">The original relational operator</param>
            private void LessPattern(Instruction.RelationalOperationType originalrelop)
            {
                relop = Randomizer.GetRelop();
                if (relop == Instruction.RelationalOperationType.Smaller || relop == Instruction.RelationalOperationType.SmallerOrEquals || relop == Instruction.RelationalOperationType.Equals)
                {
                    if (originalrelop == Instruction.RelationalOperationType.Greater || originalrelop == Instruction.RelationalOperationType.GreaterOrEquals || originalrelop == Instruction.RelationalOperationType.Equals)
                        JumpType = BlockJumpType.False;
                    else JumpType = BlockJumpType.True;
                }
                else JumpType = BlockJumpType.Ambigious;
            }
        }

        /// <summary>
        /// This function meshes up a conditional jump
        /// </summary>
        /// <param name="bb">The block containing the conditional jump to mesh up</param>
        private static void MeshConditionals(BasicBlock bb)
        {
            Variable var = bb.Instructions.Last().GetVarFromCondition();
            Instruction.RelationalOperationType relop = bb.Instructions.Last().GetRelopFromCondition();
            int C = bb.Instructions.Last().GetConstFromCondition();

            List<Cond> condlist = GenetateCondList(C, relop);
            BasicBlock truesucc = bb.Instructions.Last().GetTrueSucc();
            BasicBlock falsesucc = bb.Instructions.Last().GetFalseSucc();
            List<BasicBlock> generatedblocks = GenerateBlocks(bb, var, truesucc, falsesucc, condlist);
            
            generatedblocks.Last().LinkToSuccessor(falsesucc);
            bb.Instructions.Remove(bb.Instructions.Last());
            bb.LinkToSuccessor(generatedblocks.First(), true);
            
            /// TODO:   Generate the blocks
            ///         Generate the jumps between them, and the True-False lanes (polyrequ blocks also)
            ///         Insert the meshed control flow trnasition to the original place
        }

        /// <summary>
        /// This function generates the BasicBlocks into the function, based on the condition list
        /// </summary>
        /// <param name="bb">The actual basicblock with the conditional jump at the end</param>
        /// <param name="truelane">The true successor</param>
        /// <param name="falselane">The false successor</param>
        /// <param name="condlist">The condition list</param>
        /// <returns>The list of the generated BasicBlocks</returns>
        private static List<BasicBlock> GenerateBlocks(BasicBlock bb, Variable var, BasicBlock truelane, BasicBlock falselane, List<Cond> condlist)
        {
            List<BasicBlock> bblist = new List<BasicBlock>();
            foreach (Cond c in condlist)
            {
                bblist.Add(new BasicBlock(bb.parent));
            }
            for (int i = 0; i < condlist.Count(); i++)
            {
                switch (condlist[i].JumpType)
                {
                    case Cond.BlockJumpType.True:
                    case Cond.BlockJumpType.Last:
                        if (i != condlist.Count() - 1)
                            bblist[i].LinkToSuccessor(bblist[i + 1]);
                        else
                            bblist[i].LinkToSuccessor(falselane);
                        bblist[i].Instructions.First().MakeConditionalJump(var, condlist[i].value, condlist[i].relop, truelane);
                        break;
                    case Cond.BlockJumpType.False:
                        if (i != condlist.Count() - 1)
                            bblist[i].LinkToSuccessor(bblist[i + 1]);
                        else
                            bblist[i].LinkToSuccessor(falselane);
                        bblist[i].Instructions.First().MakeConditionalJump(var, condlist[i].value, condlist[i].relop, falselane);
                        break;
                    case Cond.BlockJumpType.Ambigious:
                        bblist[i].LinkToSuccessor(falselane);
                        bblist[i].Instructions.First().MakeConditionalJump(var, condlist[i].value, condlist[i].relop, bblist[i + 1]);
                        break;
                    default:
                        break;
                }
            }

            return bblist;
        }

        /// <summary>
        /// Shuffles the content of a generic list, with the randomizer
        /// </summary>
        /// <typeparam name="T">The contents of the list</typeparam>
        /// <param name="condlist">The list itself</param>
        /// <returns>The new, shuffled list</returns>
        private static List<T> ShuffleList<T>(List<T> condlist)
        {
            List<int> randomlist = Randomizer.GetMultipleNumbers(condlist.Count(), 0, condlist.Count()-1, false, false);
            List<T> newlist = new List<T>();
            foreach (int i in randomlist)
            {
                newlist.Add(condlist[i]);
            }
            return newlist;
        }

        /// <summary>
        /// This function repositions the Cond or Conds with JumpType Last behind the last of the ambigous ones in the list
        /// </summary>
        /// <param name="condlist">A list of Conds to reorganize</param>
        private static void RepositionLasts(List<Cond> condlist)
        {
            
            List<Cond> lasts = GetLasts(condlist);
            if (lasts.Count() == 2) lasts.First().JumpType = Cond.BlockJumpType.Ambigious;
            foreach (Cond c in lasts)
            {
                condlist.Remove(c);
            }
            int lastambigous = FindLastAmb(condlist);
            condlist.InsertRange(lastambigous + 1, lasts);
        }

        /// <summary>
        /// Finds the Conds which has JumpType Last
        /// </summary>
        /// <param name="condlist">The list of the Conds</param>
        /// <returns>A list of the required Conds</returns>
        private static List<Cond> GetLasts(List<Cond> condlist)
        {
            List<Cond> returnlist = new List<Cond>();
            foreach (Cond c in condlist)
            {
                if (c.JumpType == Cond.BlockJumpType.Last)
                {
                    returnlist.Add(c);
                }
            }
            return returnlist;
        }

        /// <summary>
        /// Finds the last ambigus Cond in a list
        /// </summary>
        /// <param name="constlist">The list to find the Cond in</param>
        /// <returns>The index of the last ambigous Cond, or if no such, the last Cond</returns>
        private static int FindLastAmb(List<Cond> constlist)
        {
            int ret = constlist.Count() -1;
            for (int i = 0; i < constlist.Count(); i++)
            {
                if (constlist[i].JumpType == Cond.BlockJumpType.Ambigious)
                    ret = i;
            }
            return ret;
        }

        /// <summary>
        /// Generates the surrounding CondConstants around a specific constant
        /// </summary>
        /// <param name="originalconstant">The actual constant which is the base of the generation</param>
        /// <param name="originalrelop">The original relational operator</param>
        /// <param name="num">The number of constants to generate (by default, it equals to 6)</param>
        /// <returns>The litst of the generated CondConstants</returns>
        private static List<Cond> GenetateCondList(int originalconstant, Instruction.RelationalOperationType originalrelop, int num = 6)
        {
            List<Cond> returnlist = new List<Cond>();
            for (int i = -1, n = 0; n < num; n++)
            {

                returnlist.Add(new Cond(originalconstant + i, originalrelop, originalconstant));

                if (i > 0) i++;
                else if (i < -1) i--;
                i *= -1;
            }
            returnlist = ShuffleList<Cond>(returnlist);
            RepositionLasts(returnlist);
            return returnlist;
        }

        /// <summary>
        /// The interface function, that meshes up the functions of a routine
        /// </summary>
        /// <param name="rtn">The routine to be meshed up</param>
        public static void MeshingAlgorithm(Routine rtn)
        {
            foreach (Function func in rtn.Functions)
            {
                MeshFunction(func);
            }
        }
    }
}


#endif
#if PSEUDOCODE 

void MeshFunction( Function actualfunction )
{
    /*
     * The getBBCunJumps returns a list of the basic blocks that has one
     * successor, which also has a sucessor. Not all of them, but some,
     * depending on the CFTRatio.
     */
    
    list<BasicBlock> basicblocks = Function.getBBUncJumps( actualfunction );
    // TODO: Ebből az alhalmazt, Ratio alapján
    /*
     * Now we go through the list, and mesh the jumps.
     */
    
    foreach ( BasicBlock bb in basicblocks )
        MeshUnconditional( bb );
    
    /*
     * Now we need the list of the conditional jumps.
     */
    
    basicblocks = Function.getBBCondJumps( actualfunction );
    
    /*
     * And go through the list of conditional jumps as well.
     */
    
    foreach ( BasicBlock bb in basicblocks )
        MeshConditional( bb );
}

/*
 * The MeshUncoditional function gets a BasicBlock, and creates
 * the Control Flow Transition.
 */

void MeshUnconditional( BasicBlock actualbb )
{
    ///* First we insert an entry point. */
    
    //BasicBlock ep = InsertEntryPoint( actualbb );
    
    ///* Next, we insert the fake flow, with one fake block, and
    // * we create a copy of the successor of the actual basic block
    // * (if needed). */
    
    //InsertFakeFlow( actualbb );
    
    ///* Finally comes the dead flow, with 3 blocks. They are all dead. */
    
    //InsertDeadFlow( ep );


    
    

    return;
    

}

/*
 * The InsertEntryPoint is the function that inserts the fake basic block
 * which will serve as the entry point of the CFT.
 */
void InsertEntryPoint( BasicBlock bb )
{
    /* We need a new BB. */

    BasicBlock ep = new BasicBlock();

    /* And we need to ad it to the function. */

    bb.parent.BasicBlocks.Add( ep );
    //parent...
    /* A function now creates an instruction with a fake conditional jump,
     * and it is fake because it always continues in the true way. */

    Instruction i = CreateFakeCondJump( bb.Instructions[0].getID() );

    /* Appending the instruction to the block we just created. */
    
    ep.Append( i );

    /* And setting its successor to the actual block. */

    ep.getSuccessors.Add( bb );

    /* Now we use the ChangeGotos function, and it changes all goto-s in the
     * function with one ID, to have an other ID */

    bb.parent.ChangeGotos( bb.Instructions[0].getID(), ep.Instructions[0].getID() );

    /* And finally we set the bacisblocks' predecessors and successors, so
     * the edges in the CFG. */
    
    foreach ( BasicBlock pred in pp.getPredecessors )
    {
        pred.getSuccessors.Delete( bb );
        pred.getSuccessors.Add( ep );
        bb.getPredecessors.Delete( pred );
    }
    bb.Predecessors.Add( ep );   
}

/*
 * Now we can proceed to the second step, wich means inserting the fake side of the
 * meshed control flow. The fake path looks like this:
 * 
 *           ------------------
 *          | Fake entry block |
 *           ------------------
 *        (True) /
 *          -----------
 *         | Actual BB |
 *          -----------
 *    (False) /   \ (True)
 *    ------------ \
 *   | Fake Block | |
 *    ------------ /
 *   |  Actual    |
 *   | Successor  |
 *    ------------
 *    
 * Since we have the injected entry point, this part of the algorythm includes changing
 * the unconditional jump of the actual basicblock into a conditional jump, with the
 * following ends: The true lane must go to the actual successor and the false must go
 * to the fake block. (Because of the future code generation, it cannot be solved that
 * the true lane would be followed immediatelly by the false...)
 */

void InsertFakeFlow( BasicBlock bb )
{
    // Using:   AppendTo( BasicBlock, Instruction );
    //          ChangeToConditional( BasicBlock, BasicBlock, Instruction, TrueEnum TrueOnly, ... );

    BasicBlock fake1 = AppendTo( actualbb );
    BasicBlock fake2 = AppendTo( actualbb, fake1.Instructions[0] );
    
    ChangeToConditional( fake2, actualbb, fake1.Instructions[0], random );

    FillWithFake( fake1 );
    FllWithFake( fake2 );
}

/*
 * Todo: somehow change an unconditional jump to a conditional:
 * Notes:
 * Generating the condition is the key.
 * Not sure how the dead variables are represented...
 * We might generate a random reloperand, and choose two random
 * dead variables...
 */
ChangeToConditional( Instruction );

/*
 * Now here comes the dead path generation.
 * It needs the entry point for parameter.
 */

/*
 * Now here comes the dead part of the control flow transition,
 * wich looks like this:
 * 
 *            ------------------
 *          | Fake entry block |
 *           ------------------
 *                           \ (False)
 *                           ---------
 *                          |   bb1   | 
 *                           ---------
 *                      (True) /    \ (False)
 *                         ------  ------
 *                        | bb2  || bb3  |
 *                         ------  ------
 *                            |       |
 *                        (Random) (Random)
 * 
 * 
 */

void InsertDeadFlow( BasicBlock pre )
{
    // Using    ChangeToConditional( BasicBlock, BasicBlock, Instruction, TrueEnum TrueOnly, ... );
    //          AppendTo( BasicBlock, Instruction );
    //          RandomJumperBlock( Function );    

    BasicBlock dead1 = RandomJumperBlock( pre.parent );
    BasicBlock dead2 = RandomJumperBlock( pre.parent );

    ChangeToConditional( pre, dead1, dead1.Instructions[0], trueonly );

    BasicBlock dead3 = AppendTo( dead1, dead1.Instructions[0] );
    ChangeToConditional( dead3, dead1.Instructions[0], random );
    
}

/*
 * So now, we have the full CFT of the unconditional jump:
 * 
 *           ------------------
 *          | Fake entry block |
 *           ------------------
 *      (True) /             \ (False)
 *        -----------       ----------
 *       | Actual BB |      |   bb1   | 
 *        -----------       ----------
 *  (False) /   \ (True) (True) /    \ (False)
 *  ------------ \          ------  ------
 * | Fake Block | |         | bb2  || bb3  |
 *  ------------ /          ------  ------
 * |  Actual    |              |       |
 * | Successor  |          (Random) (Random)
 *  ------------
 *  
 */

/*
 * Now the next step is the conditional jump, which will be a tough one,
 * if I guess right. I start with analyzing the sheet wich describes the
 * method of meshing the conditional jumps, and later on I will try to create
 * the algorithm itself.
 */

enum Relop
	{
	    equ,
        notequ,
        great,
        greatequ,
        less,
        lessequ
	}

struct K
    {
		int i;
        bool usable_relops[6];
        bool used;
        K(int i) : i(i), used(false)
        {
            for (int i=0; i<6; i++)
                usable_relops[i] = true;
        }
	}

void MeshConditional( BasicBlock bb )
{
    int C = ReadConst( bb.Instructions.Last() );
    Relop r = ReadRelop( bb.Instructions.Last() );
    // Select m;
    m = 5; // At first we choose a constant number.
    
    // Generating the constants based on C
    list<K> KList;
    GenerateList( KList, C);
    
    // Iterating K[i] 
    while ( !AllUsed( KList ) )
    {
        K selected = Select_i(KList);
        Relop act_r = SelectUsableRelop( selected );
    }
}

/* Still to do:
 * 
 * void InsertFakeFlow(...); <- Done, details needed
 * void InsertDeadFlow(...); <- Done, same situation
 * 
 * These details include the discussion of the way helper functions
 * will work, and so on.
 * 
 * void MeshConditional(); <- Started brainstorming
 */

#endif