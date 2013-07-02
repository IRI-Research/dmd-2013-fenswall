using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Kinect;

namespace MvtKinect.Segments
{
    class SwipeUp1 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {

            if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.HipCenter].Position.X)
            {
                //test pour savoir si la 
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.HipCenter].Position.Y)
                {
                    //Debug.WriteLine("Swipe 1 - main droite en dessous de la taille a droite du corps - OK");
                    return GesturePartResult.Suceed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }

    class SwipeUp2 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {
            if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.HipCenter].Position.X)
            {
                if (skeleton.Joints[JointType.HandRight].Position.Y < skeleton.Joints[JointType.ShoulderCenter].Position.Y && (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.HipCenter].Position.Y))
                {
                    // hand right of elbow

                    //Debug.WriteLine("swipe 2 - main droite entre la taille et les epaule a droite du corps - OK");
                    return GesturePartResult.Suceed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }


    class SwipeUp3 : IRelativeGestureSegment
    {
        /// <summary>
        /// Checks the gesture.
        /// </summary>
        /// <param name="skeleton">The skeleton.</param>
        /// <returns>GesturePartResult based on if the gesture part has been completed</returns>
        public GesturePartResult CheckGesture(Skeleton skeleton)
        {

            // hand right of elbow
            if (skeleton.Joints[JointType.HandRight].Position.X > skeleton.Joints[JointType.HipCenter].Position.X)
            {
                // hand above elbow
                if (skeleton.Joints[JointType.HandRight].Position.Y > skeleton.Joints[JointType.ShoulderCenter].Position.Y)
                {

                    //Debug.WriteLine("Swipe 3 - main droite au dessu des epaule a droite du corps - OK");
                    return GesturePartResult.Suceed;
                }

                // hand has not dropped but is not quite where we expect it to be, pausing till next frame
                return GesturePartResult.Pausing;
            }

            // hand dropped - no gesture fails
            return GesturePartResult.Fail;
        }
    }
}
