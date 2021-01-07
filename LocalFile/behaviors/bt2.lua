local __bt__ = {
  name= "rootNode",
  posX= 520.0,
  posY= 0.0,
  data= {
    restart= 1
  },
  children= {
    {
      name= "selectorNode",
      type= "composites",
      posX= 520.0,
      posY= 120.0,
      children= {
        {
          name= "parallelNode",
          type= "composites",
          posX= 390.0,
          posY= 240.0,
          children= {
            {
              name= "speakNode",
              type= "actions",
              posX= 390.0,
              posY= 360.0,
              data= {
                say= "hello world"
              },
            }
          }
        },
        {
          name= "parallelNode",
          type= "composites",
          posX= 520.0,
          posY= 240.0,
          children= {
            {
              name= "speakNode",
              type= "actions",
              posX= 520.0,
              posY= 360.0,
              data= {
                say= "hello world"
              },
            }
          }
        }
      }
    }
  }
}
return __bt__