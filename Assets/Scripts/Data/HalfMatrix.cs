
using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Efficient storage of an half matrix.
/// </summary>
/// <remarks>
/// This is a storage space efficient data-structure for edge weights in an undirected graph, where edges are specified as a pair of integer verticies.
///
/// Let "dAB" be the distance from point A to point B, where A and B are positive integers indexing points.
///
/// A full, unoptimzied matrix would look like this:
/// <code>
///    [ d00 d10 d20 d30 ]
///    [ d01 d11 d21 d31 ]
///    [ d02 d12 d22 d32 ]
///    [ d03 d13 d23 d33 ]
/// </code>
/// Distance to the same point is always zero, so the matrix becomes:
/// <code>
///    [   0 d10 d20 d30 ]
///    [ d01   0 d21 d31 ]
///    [ d02 d12   0 d32 ]
///    [ d03 d13 d23   0 ]
/// </code>
/// Because dXY == dYX, the matrix can be further simplified into a lower triangular matrix. Note that A is the column and B is the row.
/// <code>
///    [ 0     .   .   . ]
///    [ d01   0   .   . ]
///    [ d02 d12   0   . ]
///    [ d03 d13 d23   0 ]
/// </code>
/// Therefore, distances can be stored in a lower triangular matrix of size n-1 where A < B, with a quick check for A == B
/// <code>
///    [ d01   .   . ]
///    [ d02 d12   . ]
///    [ d03 d13 d23 ]
/// </code>
/// This can be efficiently stored as an array, where the array grows by the size of the next row for each additional dimension!
/// <code>
///    [ d01 d02 d12 d03 d13 d23 ]
/// </code>
/// This is what we call a HalfMatrix and this is the purpose of this class.
/// 
/// 
/// </remarks>
/// <typeparam name="T">the type of the matrix you want to store in each cells. for now it can only be, e.g., <c>int</c>, <c>float</c>, <c>double</c></typeparam>
public class HalfMatrix<T> where T : struct
{

    public int Population => population;
    public int RowCount => population - 1;
    private int population; // the count of our Agents
    private List<T> data = null; // the real data where all the T will be stored as an HalfMatrix list
    public int DataCount => data.Count; // the count of data we have

    // CONSTRUCTOR
    public HalfMatrix()
    {
        this.population = 0;
        this.data = new List<T>();
    }


    // DATA HANDLING
    public T this[int a, int b]
    {
        get => (a == b) ? default : this.data[data_index_of_point(a, b)];
        set
        {
            if (a == b)
            {
                throw new ArgumentOutOfRangeException(nameof(b), b, "We don't store self agent data");
            }

            this.data[data_index_of_point(a, b)] = value;
        }
    }
    public void SetData(NativeArray<T> array)
    {
        if (array.Length != this.DataCount) { throw new ArgumentException($"(HalfMatrix) Cannot set data from array of length {array.Length} because the HalfMatrix data count is {this.DataCount}"); }

        for (int i = 0; i < array.Length; i++)
        {
            this.data[i] = array[i];
        }
    }


    // AGENT HANDLING
    public void AddAgent()
    {
        // we increase the data space by the current population
        // so we can store one point per each existing agent
        // if pop == 0 then it will stay empty [] til there are at least 2 agents
        this.data.AddRange(new T[this.Population]);

        // now we have a new agent so population + 1
        this.population++;
    }
    public void RemoveAgent(int agent_index, ref string log)
    {
        if (agent_index < 0 || agent_index >= Population) { throw new ArgumentOutOfRangeException($"(HalfMatrix) Cannot remove agent at index {agent_index} because the population is {Population}"); }

        // special case of population being 1 or 0
        if (Population == 1)
        {
            this.population = 0; this.data.Clear();
            log = "(HalfMatrix) Removed the last agent, matrix is now empty";
            return;
        }

        // we get all the indexes of the data that are related to this agent
        List<int> indexes = data_indexes_of_agent(agent_index);
        log += $"(HalfMatrix) Removing agent {agent_index} data at indexes: {string.Join(", ", indexes)}\n";

        // we remove all the data related to this agent
        for (int i = indexes.Count - 1; i >= 0; i--)
        {
            int index = indexes[i];
            if (index < 0 || index >= data.Count) { throw new ArgumentOutOfRangeException($"(HalfMatrix) Cannot remove data at index {index} because the data count is {data.Count}"); }
            data.RemoveAt(index);
        }

        // we now need to decrease the population
        this.population--;
    }

    // GETTERS
    private int main_row_of_agent(int agent_index) => agent_index - 1;
    private int row_length(int row) => row + 1;
    private int get_first_row_index(int row) => row * (row + 1) / 2;
    private int data_index_of_point(int a, int b)
    {
        // check if agent_ids are correct
        if (a == b) { throw new ArgumentOutOfRangeException("we don't store self agent data"); }
        else if (a < 0 || a >= population) { throw new ArgumentOutOfRangeException(nameof(a), a, "Must be greater than 0 and less than the population : " + population); }
        else if (b < 0 || b >= population) { throw new ArgumentOutOfRangeException(nameof(b), b, "Must be greater than 0 and less than the population : " + population); }

        // get the max_agent 
        int max_agent = (a < b) ? b : a; // max_agent is maximum
        int min_agent = (a < b) ? a : b; // min_agent is minimum
        return get_first_row_index(main_row_of_agent(max_agent)) + min_agent;
    }
    private List<int> data_indexes_of_agent(int agent_index)
    {
        List<int> indexes = new List<int>();
        int main_row = main_row_of_agent(agent_index);
        int main_row_index = get_first_row_index(main_row);

        // we add the main_row indexes to the indexes
        for (int i = 0; i < row_length(main_row); i++)
        {
            indexes.Add(main_row_index + i);
        }

        // we now add all the indexes of the agent in the above rows
        for (int r = main_row + 1; r < RowCount; r++)
        {
            int row_index = get_first_row_index(r);
            indexes.Add(row_index + agent_index);
        }

        return indexes;
    }

}