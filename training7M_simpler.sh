#!/bin/bash

# 0 Fantasmi
mlagents-learn configs/final/MLA-training-IT.yaml --run-id seq3_0G_1L_IL --env Environments/0/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_0G_1L_F --init seq3_0G_1L_IL --env Environments/0/Pacman

# 1 Fantasmini
mlagents-learn configs/final/MLA-training-IT_1.yaml --run-id seq3_1G_1L_IL --init seq3_0G_1L_F --env Environments/1/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_1G_1L_F --init seq3_1G_1L_IL --env Environments/1/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_1G_3L_F --init seq3_1G_1L_F --env Environments/1_3/Pacman

# 2 Fantasmini
mlagents-learn configs/final/MLA-training-IT_2.yaml --run-id seq3_2G_1L_IL --init seq3_1G_3L_F --env Environments/2/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_2G_1L_F --init seq3_2G_1L_IL --env Environments/2/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_2G_3L_F --init seq3_2G_1L_F --env Environments/2_3/Pacman

# 3 Fantasmini
mlagents-learn configs/final/MLA-training-IT_3.yaml --run-id seq3_3G_1L_IL --init seq3_2G_3L_F --env Environments/3/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_3G_1L_F --init seq3_3G_1L_IL --env Environments/3/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_3G_3L_F --init seq3_3G_1L_F --env Environments/3_3/Pacman

# 4 Fantasmini
mlagents-learn configs/final/MLA-training-IT_4.yaml --run-id seq3_4G_1L_IL --init seq3_3G_3L_F --env Environments/4/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_4G_1L_F --init seq3_4G_1L_IL --env Environments/4/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq3_4G_3L_F --init seq3_4G_1L_F --env Environments/4_4/Pacman
