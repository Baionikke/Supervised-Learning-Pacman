#!/bin/bash

# 0 Fantasmi
mlagents-learn configs/finalb/MLA-training-IT_0.yaml --run-id seq4_0G_1L_IL --env Environments/final/0/Pacman
mlagents-learn configs/finalb/MLA-training.yaml --run-id seq4_0G_1L_F --init seq4_0G_1L_IL --env Environments/final/0/Pacman

# 1 Fantasmini
mlagents-learn configs/finalb/MLA-training-IT_1.yaml --run-id seq4_1G_1L_IL --init seq4_0G_1L_F --env Environments/final/1/Pacman
mlagents-learn configs/finalb/MLA-training.yaml --run-id seq4_1G_1L_F --init seq4_1G_1L_IL --env Environments/final/1/Pacman
mlagents-learn configs/finalb/MLA-training-IT_1-3.yaml --run-id seq4_1G_3L_IL --init seq4_1G_1L_F --env Environments/final/1_3/Pacman

# 2 Fantasmini
mlagents-learn configs/finalb/MLA-training-IT_2.yaml --run-id seq4_2G_1L_IL --init seq4_1G_3L_IL --env Environments/final/2/Pacman
mlagents-learn configs/finalb/MLA-training.yaml --run-id seq4_2G_1L_F --init seq4_2G_1L_IL --env Environments/final/2/Pacman
mlagents-learn configs/finalb/MLA-training-IT_2-3.yaml --run-id seq4_2G_3L_IL --init seq4_2G_1L_F --env Environments/final/2_3/Pacman

# 3 Fantasmini
mlagents-learn configs/finalb/MLA-training-IT_3.yaml --run-id seq4_3G_1L_IL --init seq4_2G_3L_IL --env Environments/final/3/Pacman
mlagents-learn configs/finalb/MLA-training.yaml --run-id seq4_3G_1L_F --init seq4_3G_1L_IL --env Environments/final/3/Pacman
mlagents-learn configs/finalb/MLA-training-IT_3-3.yaml --run-id seq4_3G_3L_IL --init seq4_3G_1L_F --env Environments/final/3_3/Pacman

# 4 Fantasmini
mlagents-learn configs/finalb/MLA-training-IT_4.yaml --run-id seq4_4G_1L_IL --init seq4_3G_3L_IL --env Environments/final/4/Pacman
mlagents-learn configs/finalb/MLA-training.yaml --run-id seq4_4G_1L_F --init seq4_4G_1L_IL --env Environments/final/4/Pacman
mlagents-learn configs/finalb/MLA-training-IT_4-3.yaml --run-id seq4_4G_3L_IL --init seq4_4G_1L_F --env Environments/final/4_3/Pacman
