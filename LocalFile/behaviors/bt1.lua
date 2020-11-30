local __bt__ = {
  name= "rootNode",
  posX= 450.0,
  posY= 50.0,
  data= {
    restartOnComplete= 1
  },
  children= {
    {
      name= "randomSelectorNode",
      type= "composites",
      posX= 450.0,
      posY= 200.0,
      children= {
        {
          name= "selectorNode",
          type= "composites",
          posX= 450.0,
          posY= 350.0,
          children= {
            {
              name= "sequenceNode",
              type= "composites",
              posX= 450.0,
              posY= 500.0,
              children= {
                {
                  name= "runAnimatorNode",
                  type= "actions",
                  posX= 450.0,
                  posY= 650.0,
                }
              }
            }
          }
        }
      }
    }
  }
}
return __bt__