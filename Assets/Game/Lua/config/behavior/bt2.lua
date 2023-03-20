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
          name= "parallelNode",
          type= "composites",
          children= {
            {
              name= "speakNode",
              type= "actions",
              data= {
                say= "hello world"
              },
            }
          }
        },
        {
          name= "parallelNode",
          type= "composites",
          children= {
            {
              name= "speakNode",
              type= "actions",
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