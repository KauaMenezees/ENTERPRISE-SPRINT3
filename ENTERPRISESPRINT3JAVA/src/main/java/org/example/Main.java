package org.example;

import java.io.BufferedReader;
import java.io.File;
import java.io.FileReader;
import java.io.IOException;

public class Main {
    public static void main(String[] args) {
        //Colocar aqui o caminho do arquivo CSV do C#
        String csvFilePath = "D:\\SP3\\ENTERPRISESPRINT3\\ENTERPRISESPRINT3\\bin\\Debug\\net7.0\\cienalab_product.csv";

        File file = new File(csvFilePath);
        if (!file.exists()) {
            System.out.println("O arquivo CSV não foi encontrado.");
            return;
        }

        try (BufferedReader br = new BufferedReader(new FileReader(csvFilePath))) {
            String line;
            while ((line = br.readLine()) != null) {
                String[] columns = line.split(",");

                for (String column : columns) {
                    System.out.println(column.trim());
                }
            }
        } catch (IOException e) {
            e.printStackTrace();
        }
    }
}