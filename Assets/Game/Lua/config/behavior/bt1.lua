local __bt__ = {
  name= "rootNode",
  data= {
    restart= 1
  },
  children= {
    {
      name= "selectorNode",
      type= "composites",
      data= {
        abort= "None"
      },
      children= {
        {
          name= "checkStateNode",
          type= "decorators",
          data= {
            stateId= 0
          },
          children= {
            {
              name= "sequenceNode",
              type= "composites",
              data= {
                abort= "Self"
              },
              children= {
                {
                  name= "weightNode",
                  type= "actions",
                  data= {
                    weight= 10
                  },
                },
                {
                  name= "parallelNode",
                  type= "composites",
                  data= {
                    abort= "None"
                  },
                  children= {
                    {
                      name= "waitNode",
                      type= "actions",
                      data= {
                        waitMin= 2,
                        waitMax= 5
                      },
                    }
                  }
                }
              }
            }
          }
        },
        {
          name= "checkStateNode",
          type= "decorators",
          data= {
            stateId= 0
          },
          children= {
            {
              name= "moveToPositionNode",
              type= "actions",
            }
          }
        },
        {
          name= "checkStateNode",
          type= "decorators",
          data= {
            stateId= 0
          },
          children= {
            {
              name= "moveToPositionNode",
              type= "actions",
            }
          }
        }
      }
    }
  }
}
return __bt__