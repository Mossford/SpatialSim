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

## [DONE] Swap to SDL3 for windowing

## Make texture loading threaded

## Implement more texture methods

## Add unlit and lit shaders plus more types

## Texture usage can be or together

## Mouse stops camera movement when cursor reaches edge of screen
### Allow mouse delta to still be calculated even if mouse is stopped by edge of screen

## Look into if uniform data would be possible on post processes or at least push constants