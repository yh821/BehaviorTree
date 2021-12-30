local __bt__ = {
  name= "rootNode",
  posX= 520.0,
  posY= 120.0,
  data= {
    restart= 1
  },
  children= {
    {
      name= "selectorNode",
      type= "composites",
      posX= 520.0,
      posY= 240.0,
      children= {
        {
          name= "checkStateNode",
          type= "decorators",
          posX= 390.0,
          posY= 360.0,
          data= {
            stateId= 0
          },
          children= {
            {
              name= "sequenceNode",
              type= "composites",
              posX= 390.0,
              posY= 480.0,
              children= {
                {
                  name= "weightNode",
                  type= "actions",
                  posX= 260.0,
                  posY= 600.0,
                  data= {
                    weight= 10
                  },
                },
                {
                  name= "parallelNode",
                  type= "composites",
                  posX= 390.0,
                  posY= 600.0,
                  children= {
                    {
                      name= "waitNode",
                      type= "actions",
                      posX= 390.0,
                      posY= 720.0,
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
          posX= 520.0,
          posY= 360.0,
          data= {
            stateId= 0
          },
          children= {
            {
              name= "moveToPositionNode",
              type= "actions",
              posX= 520.0,
              posY= 480.0,
            }
          }
        },
        {
          name= "checkStateNode",
          type= "decorators",
          posX= 650.0,
          posY= 360.0,
          data= {
            stateId= 0
          },
          children= {
            {
              name= "moveToPositionNode",
              type= "actions",
              posX= 650.0,
              posY= 480.0,
            }
          }
        }
      }
    }
  }
}
return __bt__