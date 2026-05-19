# Bugs

## [FIXED?] Imgui does not recreate swapchain to correct size for dpi scaling 
### Text does get blurry with higher dpi or scaling factor so different change could be made than just setting the framebuffer scale?

# To Do

## [DONE] Pipeline layers
### Sort mesh renderers by pipeline layers, sort when meshrenderer added or layer change

## Pipeline vertex attributes need abstracting

## [DONE] PostProcess and RenderTextures
### Make it easier to get rendered images at some point
### Normal world render goes to some texture, post process grabs this and runs potentiall multiple times, and on last render it goes to swapchain

## Swap to SDL3 for windowing

## Make texture loading threaded