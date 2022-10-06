#!/bin/bash

# 0 Fantasmi
mlagents-learn configs/final/MLA-training-IT_0.yaml --run-id seq2_0G_1L_IL --env Environments/0/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq2_0G_1L_F --init seq2_0G_1L_IL --env Environments/0/Pacman

# 1 Fantasmini
mlagents-learn configs/final/MLA-training-IT_1.yaml --run-id seq2_1G_1L_IL --init seq2_0G_1L_F --env Environments/1/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq2_1G_1L_F --init seq2_1G_1L_IL --env Environments/1/Pacman
mlagents-learn configs/final/MLA-training-IT_1-3.yaml --run-id seq2_1G_3L_F --init seq2_1G_1L_F --env Environments/1_3/Pacman

# 2 Fantasmini
mlagents-learn configs/final/MLA-training-IT_2.yaml --run-id seq2_2G_1L_IL --init seq2_1G_3L_F --env Environments/2/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq2_2G_1L_F --init seq2_2G_1L_IL --env Environments/2/Pacman
mlagents-learn configs/final/MLA-training-IT_2-3.yaml --run-id seq2_2G_3L_F --init seq2_2G_1L_F --env Environments/2_3/Pacman

# 3 Fantasmini
mlagents-learn configs/final/MLA-training-IT_3.yaml --run-id seq2_3G_1L_IL --init seq2_2G_3L_F --env Environments/3/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq2_3G_1L_F --init seq2_3G_1L_IL --env Environments/3/Pacman
mlagents-learn configs/final/MLA-training-IT_3-3.yaml --run-id seq2_3G_3L_F --init seq2_3G_1L_F --env Environments/3_3/Pacman

# 4 Fantasmini
mlagents-learn configs/final/MLA-training-IT_4.yaml --run-id seq2_4G_1L_IL --init seq2_3G_3L_F --env Environments/4/Pacman
mlagents-learn configs/final/MLA-training.yaml --run-id seq2_4G_1L_F --init seq2_4G_1L_IL --env Environments/4/Pacman
mlagents-learn configs/final/MLA-training-IT_4-3.yaml --run-id seq2_4G_3L_F --init seq2_4G_1L_F --env Environments/4_3/Pacman
