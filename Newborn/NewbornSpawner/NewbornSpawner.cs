using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Gene
{
  public class NewbornSpawner : MonoBehaviour
  {
    private float timer = 0.0f;
    public void Update()
    {
      timer += Time.deltaTime;
      if (timer > 10f)
      {
        StartCoroutine(NewbornService.ListNewborn());
        timer = 0.0f;
      }
    }
  }
}