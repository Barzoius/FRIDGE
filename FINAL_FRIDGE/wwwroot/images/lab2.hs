import Data.List

poly :: Integer -> Integer -> Integer -> Integer -> Integer
poly a b c x = a*x*x+b*x+c

eeny :: Integer -> [Char]
eeny x = if(mod x 2 == 0)
                then "eeny"
                else "meeny"

fizzbuzz :: Int -> String
fizzbuzz n | n `mod` 3 == 0  = "Fizz"
       | n `mod` 5  == 0  = "Buzz"
       | n `mod` 15  == 0  = "FizzBuzz"


fib :: Integer -> Integer
fib n | n < 2 = n
      | otherwise = fib (n - 1) + fib (n - 2)       

fib_ecuational :: Integer -> Integer
fib_ecuational 0 = 0
fib_ecuational 1 = 1
fib_ecuational n = fib_ecuational(n-1) + fib_ecuational(n-2)

tribonacci :: Integer -> Integer
tribonacci 1 = 1 
tribonacci 2 = 1
tribonacci 3 = 2
tribonacci n = tribonacci(n - 1) + tribonacci(n - 2) + tribonacci(n - 3) 

factorial :: Integer -> Integer   
factorial x = if x == 1 then 1 else x * factorial(x-1)    

binomial :: Integer -> Integer -> Integer
binomial n 0 = 1
binomial 0 k = 0
binomial n k = factorial n `div` (factorial k * factorial (n-k))


llength :: [Integer] -> Integer
llength [] = 0 
llength (h:t) = 1 + llength t

verifL :: [Integer] -> Bool
verifL [] = True
verifL a = if(llength a `mod` 2 == 0) then True else False

-- verifL :: [Int] -> Bool
-- verifL = even . length

-- takefinal :: [Int] -> Int -> [Int]
-- takefinal a n = if(llength a <= n ) then a else takefinal (tail  a) n

takefinal :: [Int] -> Int -> [Int]
takefinal x n
  | n <= 0 = []
  | length x <= n = x
  | otherwise = takefinal (tail x) n

remove :: [Int] -> Int -> [Int]  
remove x n = take (n-1) x ++ drop n x


         

                        
