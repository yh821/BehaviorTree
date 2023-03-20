local __bt__ = {
  name= "rootNode",
  data= {
    restart= 1
  },
  children= {
    {
      name= "selectorNode",
      type= "composites",
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