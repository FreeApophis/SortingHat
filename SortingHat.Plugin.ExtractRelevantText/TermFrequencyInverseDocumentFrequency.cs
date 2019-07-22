using System;

namespace SortingHat.Plugin.ExtractRelevantText
{
    public static class TermFrequencyInverseDocumentFrequency
    {
        public static double TermFrequency(int occurencesOfWord, int totalWordCount)
        {
            return 1.0 * occurencesOfWord / totalWordCount;
        }

        public static double InverseDocumentFrequency(int containingDocuments, int totalNumberOfDocuments)
        {
            return Math.Log10(1.0 * totalNumberOfDocuments / containingDocuments);
        }

        public static double TfIdf(int occurencesOfWord, int totalWordCount, int containingDocuments, int totalNumberOfDocuments)
        {
            return TermFrequency(occurencesOfWord, totalWordCount) * InverseDocumentFrequency(containingDocuments, totalNumberOfDocuments);
        }
    }
}
