using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class KartAgent_Drift : Agent
{
  public CheckpointManager_Drift _checkpointManager;
  private KartController_Drift _kartController;

  public override void Initialize()
  {
    _kartController = GetComponent<KartController_Drift>();
  }

  public override void OnEpisodeBegin()
  {
    _checkpointManager.ResetCheckpoints();
    _kartController.Respawn();
  }

  public override void CollectObservations(VectorSensor sensor)
  {
    Vector3 diff = _checkpointManager.nextCheckPointToReach.transform.position - transform.position;
    sensor.AddObservation(diff / 20f);
    AddReward(-0.001f);
  }

  public override void OnActionReceived(ActionBuffers actions)
  {
    var Actions = actions.ContinuousActions;

    int AccAction = Actions[1] > 0 ? 1 : 0;
    _kartController.ApplyAcceleration(AccAction);


    _kartController.Steer(Actions[0]);


    int JumpAction = Actions[2] > 0 ? 1 : -1;
    if (JumpAction == 1 && !_kartController.drifting && Actions[0] != 0)
    {
      _kartController.DriftStart(JumpAction);
    }

    if (JumpAction == -1 && _kartController.drifting)
    {
      _kartController.Boost();
    }
  }

public override void Heuristic(in ActionBuffers actionsOut)
{
  var continousActions = actionsOut.ContinuousActions;
  continousActions.Clear();

  continousActions[0] = Input.GetAxis("Horizontal");
  continousActions[1] = Input.GetKey(KeyCode.W) ? 1f : 0f;
}
}
