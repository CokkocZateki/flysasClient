﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;


namespace FlysasClient
{
    public class ParserException : Exception
    {
        public ParserException(string s) : base(s) { }
    }

    public class Parser
    {
        Regex airportExp = new Regex("[a-zA-Z]+");
        public SASQuery Parse(string input)
        {
            var splitChars = new[] { ' ', ',', '-' };
            var parts = input.Split(splitChars, StringSplitOptions.RemoveEmptyEntries);
            var stack = new Stack<string>(parts.Reverse());
            SASQuery request = null;
            if (parts.Length > 2)
            {
                request = new SASQuery();
                request.From = stack.Pop().ToUpper();
                if (airportExp.IsMatch(stack.Peek()))
                {
                    request.To = stack.Pop().ToUpper();
                    if (airportExp.IsMatch(stack.Peek()))
                    {
                        request.ReturnFrom = stack.Pop().ToUpper();
                        if (airportExp.IsMatch(stack.Peek()))
                        {

                            //req.ReturnTo = stack.Pop().ToUpper();
                            var tmp = stack.Pop().ToUpper();
                            if (tmp != request.From)
                                throw new ParserException("Must return to origin");
                        }
                    }
                }
                request.OutDate = parseDate(stack.Pop(), null);
                if (stack.Any())
                {
                    request.InDate = parseDate(stack.Pop(), request.OutDate);
                }
            }
            else
                throw new ParserException("To few arguments");
            return request;
        }

        DateTime parseDate(string s, DateTime? relativeTo)
        {
            if (s != null && s.ToLower() == "=")
                return DateTime.Now.Date;
            if (relativeTo.HasValue)
            {
                var regex = new System.Text.RegularExpressions.Regex("\\+(\\d+)(.?)");
                var res = regex.Match(s);
                if (res.Success)
                {
                    string unit = "d";
                    int val = int.Parse(res.Groups[1].Captures[0].Value);
                    if (res.Groups.Count > 2)
                        unit = res.Groups[2].Captures[0].Value.ToLower();
                    if (unit == "m")
                        return relativeTo.Value.AddMonths(val);
                    if (unit == "w")
                        val *= 7;
                    return relativeTo.Value.AddDays(val);
                }
            }
            uint d, m;
            var parts = s.Split('/');
            if (parts.Length == 2 && uint.TryParse(parts[0], out d) && uint.TryParse(parts[1], out m))
            {

                try
                {
                    var date = new DateTime(DateTime.Now.Year, (int)m, (int)d);
                    if (date.Date < DateTime.Now.Date)
                        date = date.AddYears(1);
                    return date;
                }
                catch
                {
                    throw new ParserException("Invalid date format");
                }
            }
            return DateTime.Parse(s);
        }
    }
}