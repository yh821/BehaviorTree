local __bt__ = {
  name= "rootNode",
  posX= 460.0,
  posY= 50.0,
  data= {
    restart= 1
  },
  children= {
    {
      name= "selectorNode",
      type= "composites",
      posX= 460.0,
      posY= 170.0,
      children= {
        {
          name= "checkStateNode",
          type= "decorators",
          posX= 330.0,
          posY= 290.0,
          data= {
            stateId= 0
          },
          children= {
            {
              name= "sequenceNode",
              type= "composites",
              posX= 330.0,
              posY= 410.0,
              children= {
                {
                  name= "weightNode",
                  type= "actions",
                  posX= 200.0,
                  posY= 530.0,
                  data= {
                    weight= 10
                  },
                },
                {
                  name= "parallelNode",
                  type= "composites",
                  posX= 330.0,
                  posY= 530.0,
                  children= {
                    {
                      name= "waitNode",
                      type= "actions",
                      posX= 330.0,
                      posY= 650.0,
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
          posX= 590.0,
          posY= 290.0,
          data= {
            stateId= 0
          },
        },
        {
          name= "checkStateNode",
          type= "decorators",
          posX= 460.0,
          posY= 290.0,
          data= {
            stateId= 0
          },
        }
      }
    }
  }
}
return __bt__