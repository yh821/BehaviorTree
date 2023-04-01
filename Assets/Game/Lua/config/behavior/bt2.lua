local __bt__ = {
  file= "rootNode",
  data= {
    restart= 1
  },
  children= {
    {
      file= "selectorNode",
      type= "composites",
      children= {
        {
          file= "parallelNode",
          type= "composites",
          children= {
            {
              file= "speakNode",
              type= "actions",
              data= {
                say= "hello world"
              },
            }
          }
        },
        {
          file= "parallelNode",
          type= "composites",
          children= {
            {
              file= "speakNode",
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