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
 * becouse if we mesh up the CFG based on the unconditional jumps, and we try to determine
 * the conditional jumps which will locate the place of the meshing by the conditional
 * jumps, we might face the situation that for instance we only mesh up the conditional
 * jumps which are generated by us erstwhile, the original conditional jumps will remain
 * unmeshed... this explanation is getting to be too verbosed, so the point is: the first
 * step is to define the several jumps which the mesh will be based on.
 */

// Dmitriy: Agree on that, but we can easily solve it. Let's discuss it on Friday!

//void define_jumps( list<BasicBlock> unc_jmp_set, list<BasicBlock> con_jmp_set)
//{
    
//}

#endif