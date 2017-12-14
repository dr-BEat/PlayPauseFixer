# PlayPauseFixer
A small console tool that fixes the play/pause action of the Sony MDR-1000X on Windows 10.

On Windows 10 the next and previous song gestures of the MDR-1000X work fine, they basically just emulate the multimedia keys on keyboard.
However for some reason, play/pause does not, causing the gesture not to work, or only partially in some applications.

PlayPauseFixer fixes this by listening on for the play/pause command from the headphones and emulating the correct multimedia key.