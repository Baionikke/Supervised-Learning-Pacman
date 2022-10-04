# Moduli

### PacmanAgent
- [x] Osservazioni
- [x] Penalizzazione se fermo (-1)
- [ ] Penalizzazione per distanza dai fantasmi (-0.2; -0.02)
- [ ] Reward se lascia andare dritto (0.008)
- [ ] Penalizzazione svolta occupata (-0.5)
- [ ] Reward se svolta libera (0.012)
- [x] Penalizzazione inversione (-0.1)
- [x] Reward se gira in direzione dei pellet (0.5)
- [ ] Penalizzazione se gira e non ci sono pellet ma dall'altra parte sì (-2)
- [ ] Reward se va contro un fantasma spaventato (0.2)
- [ ] Reward se va nella direzione in cui ci sono più pellet (0.024)
- [ ] Penalizzazione se non va nella direzione in cui ci sono più pellet (-0.008)
- [ ] Reward se fugge frontalmente in oppisizione al fantasma (1.5)
- [ ] Penalizzazione se non fugge frontalmente in oppisizione al fantasma (-0.5)
- [ ] Reward se fugge lateralmente in opposizione al fantasma (1.5)
- [ ] Penalizzazione se va contro un fantasma ad una svolta (-0.8f)
- [ ] Reward se fugge da un fantasma vicino sui 4 quadranti e non sugli assi NSEO (1.5f)

### GameManager

- [x] Reward se mangia un fantasma (6)
- [x] Reward se vince (300)
- [x] Penalizzazione se viene mangiato (-50)
- [x] Reward **moltiplicativo** se mangia un pellet (0.006*num_pellet_eaten)
- [x] Reward se mangia un powerpellet (5; 3; -0.5)

### Rete
- PPO
- Network principale:
  - 256 neuroni per layer
  - 5 layer
  - [x] Visual encoder: NatureCNN (Camera Sensor)
  - [x] Recurrent
  - [x] Memory (LSTM):
    - Sequenza 32
    - Memoria 512
- Reward:
  - [x] Extrinsic
  - [x] Curiosity
- [x] Behavioral Cloning

### Unity
- [x] Camera sensor
- [ ] Raycast per muri e pellet
- [ ] Raycast per fantasmi  