#!/bin/bash

# 0 Fantasmi
mlagents-learn configs/seq/MLA-training-IT_0.yaml --run-id seq_0G_1L_IL --env seq/Pacman0
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_0G_1L_F --init seq_0G_1L_IL --env seq/Pacman0

# 1 Fantasmini
mlagents-learn configs/seq/MLA-training-IT_1.yaml --run-id seq_1G_1L_IL --init seq_0G_1L_F --env seq/Pacman1
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_1G_1L_F --init seq_1G_1L_IL --env seq/Pacman1
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_1G_3L_F --init seq_1G_1L_F --env seq/Pacman1_3

# 2 Fantasmini
mlagents-learn configs/seq/MLA-training-IT_2.yaml --run-id seq_2G_1L_IL --init seq_1G_3L_F --env seq/Pacman2
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_2G_1L_F --init seq_2G_1L_IL --env seq/Pacman2
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_2G_3L_F --init seq_2G_1L_F --env seq/Pacman2_3

# 3 Fantasmini
mlagents-learn configs/seq/MLA-training-IT_3.yaml --run-id seq_3G_1L_IL --init seq_2G_3L_F --env seq/Pacman3
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_3G_1L_F --init seq_3G_1L_IL --env seq/Pacman3
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_3G_3L_F --init seq_3G_1L_F --env seq/Pacman3_3

# 4 Fantasmini
mlagents-learn configs/seq/MLA-training-IT_4.yaml --run-id seq_4G_1L_IL --init seq_3G_3L_F --env seq/Pacman4
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_4G_1L_F --init seq_4G_1L_IL --env seq/Pacman4
mlagents-learn configs/seq/MLA-training.yaml --run-id seq_4G_3L_F --init seq_4G_1L_F --env seq/Pacman4_3
