local __bt__ = {
  name rootNode,
  type nil,
  posX 450.0,
  posY 50.0,
  data {
    restartOnComplete 1
  },
  children {
    {
      name randomSelectorNode,
      type composites,
      posX 450.0,
      posY 200.0,
      data nil,
      children {
        {
          name selectorNode,
          type composites,
          posX 450.0,
          posY 350.0,
          data nil,
          children {
            {
              name sequenceNode,
              type composites,
              posX 450.0,
              posY 500.0,
              data nil,
              children {
                {
                  name runAnimatorNode,
                  type actions,
                  posX 450.0,
                  posY 650.0,
                  data nil,
                  children nil
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