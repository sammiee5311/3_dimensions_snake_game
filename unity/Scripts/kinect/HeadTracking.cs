using K4AdotNet.BodyTracking;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace K4AdotNet.Samples.Unity
{
    public class HeadTracking : MonoBehaviour
    {
        private void Awake()
        {
            PrintHeadPosition();
        }

        private void PrintHeadPosition()
        {
            var IsSkeleton = FindObjectOfType<SkeletonProvider>();
            if (IsSkeleton != null)
            {
                IsSkeleton.SkeletonUpdated += SkeletonHead;
            }
        }

        private void SkeletonHead(object sender, SkeletonEventArgs e)
        {
            if (e.Skeleton != null)
            {
                var HeadPos = ConvertKinectPos(e.Skeleton.Value[JointType.Head].PositionMm);
                transform.position = HeadPos;
            }
        }

        private static Vector3 ConvertKinectPos(Float3 pos)
        {
            // Kinect Y axis points down, so negate Y coordinate
            // Scale to convert millimeters to meters
            // https://docs.microsoft.com/en-us/azure/Kinect-dk/coordinate-systems
            // Other transforms (positioning of the skeleton in the scene, mirroring)
            // are handled by properties of ascendant GameObject's
            return new Vector3(
                 -pos.X * 0.001f * 59f,
                 -pos.Y * 0.001f * 39f,
                 -pos.Z * 0.065f);
            /*           return new Vector3(
                           ((pos.X * 0.5f / (1980 - 120.0f)) - 0.5f) * 38,
                           -((pos.Y * 0.5f / (1080 - 120.0f)) - 0.5f) * 22,
                           -pos.Z * 0.05f);*/
        }
    }
}
