local __bt__ = {
  file= "RootNode",
  type= "",
  sharedData= {
    treeShared2= 2
  },
  data= {
    restart= 1
  },
  children= {
    {
      file= "SelectorNode",
      type= "composites/SelectorNode",
      data= {
        abortType= "None"
      },
      children= {
        {
          file= "SequenceNode",
          type= "composites/SequenceNode",
          data= {
            abortType= "Self"
          },
          children= {
            {
              file= "IsInViewNode",
              type= "conditions/IsInViewNode",
              data= {
                ViewRange= 12
              },
            },
            {
              file= "SelectorNode",
              type= "composites/SelectorNode",
              data= {
                abortType= "None"
              },
              children= {
                {
                  file= "SequenceNode",
                  type= "composites/SequenceNode",
                  data= {
                    abortType= "None"
                  },
                  children= {
                    {
                      file= "WeightNode",
                      type= "actions/common/WeightNode",
                      data= {
                        weight= 500
                      },
                    },
                    {
                      file= "WaitNode",
                      type= "actions/common/WaitNode",
                      data= {
                        min_time= 1,
                        max_time= 3
                      },
                    }
                  }
                },
                {
                  file= "SequenceNode",
                  type= "composites/SequenceNode",
                  data= {
                    abortType= "None"
                  },
                  children= {
                    {
                      file= "RandomPositionNode",
                      type= "actions/RandomPositionNode",
                      data= {
                        pos= "TargetPos"
                      },
                    },
                    {
                      file= "MoveToPositionNode",
                      type= "actions/MoveToPositionNode",
                      data= {
                        pos= "RandomPos"
                      },
                    }
                  }
                }
              }
            }
          }
        },
        {
          file= "MoveToPositionNode",
          type= "actions/MoveToPositionNode",
          data= {
            pos= "TargetPos",
            speed= 10
          },
        }
      }
    }
  }
}
return __bt__